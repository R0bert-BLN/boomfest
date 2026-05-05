using BoomFest.Dtos;
using BoomFest.Repositories;

namespace BoomFest.Services;

public class EventsService : IEventsService
{
    private readonly IFestivalRepository _festivalRepository;

    public EventsService(IFestivalRepository festivalRepository)
    {
        _festivalRepository = festivalRepository;
    }

    public async Task<EventsIndexDto> GetEventsAsync(string? query)
    {
        var normalized = query?.Trim() ?? string.Empty;
        var festivals = await _festivalRepository.GetPublishedFestivalsAsync(normalized);
        var soldCounts = await _festivalRepository.GetSoldTicketsByFestivalIdsAsync(festivals.Select(f => f.Id));

        var events = festivals.Select(festival =>
        {
            var totalStock = festival.TicketCategories.Sum(category => category.Stock);
            var sold = soldCounts.TryGetValue(festival.Id, out var count) ? count : 0;
            var minPrice = festival.TicketCategories.Count > 0
                ? festival.TicketCategories.Min(category => category.Price)
                : (decimal?)null;

            var location = string.IsNullOrWhiteSpace(festival.Location)
                ? string.Join(", ", new[] { festival.City, festival.Country }.Where(value => !string.IsNullOrWhiteSpace(value)))
                : festival.Location;

            return new EventCardDto
            {
                Id = festival.Id,
                Title = festival.Title,
                Location = location,
                StartDate = festival.StartDate,
                PictureUrl = festival.PictureUrl,
                MinPrice = minPrice,
                IsSoldOut = totalStock > 0 && sold >= totalStock
            };
        }).ToList();

        return new EventsIndexDto
        {
            SearchQuery = normalized,
            TotalCount = events.Count,
            Events = events
        };
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(Guid id)
    {
        var festival = await _festivalRepository.GetFestivalDetailsAsync(id);
        if (festival == null)
        {
            return null;
        }

        var soldByCategory = await _festivalRepository.GetSoldTicketsByCategoryAsync(id);

        var ticketTiers = festival.TicketCategories
            .OrderByDescending(category => category.Price)
            .Select(category =>
            {
                var sold = soldByCategory.TryGetValue(category.Id, out var count) ? count : 0;
                var remaining = Math.Max(category.Stock - sold, 0);

                return new EventTicketTierDto
                {
                    CategoryId = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Price = category.Price,
                    Remaining = remaining
                };
            })
            .ToList();

        var lineups = festival.Lineups
            .OrderBy(lineup => lineup.StartDate)
            .Select(lineup => new EventLineupDto
            {
                ArtistName = lineup.Artist.Name,
                ArtistImageUrl = lineup.Artist.PictureUrl,
                Description = lineup.Artist.Description
            })
            .ToList();

        var isSoldOut = ticketTiers.Count > 0 && ticketTiers.All(tier => tier.Remaining == 0);

        return new EventDetailsDto
        {
            Id = festival.Id,
            Title = festival.Title,
            Description = festival.Description ?? string.Empty,
            Location = festival.Location,
            StartDate = festival.StartDate,
            EndDate = festival.EndDate,
            PictureUrl = festival.PictureUrl,
            Lineups = lineups,
            TicketTiers = ticketTiers,
            IsSoldOut = isSoldOut
        };
    }
}

