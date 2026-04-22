using BoomFest.Dtos;
using BoomFest.Models;
using BoomFest.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BoomFest.Services;

public class FestivalService : IFestivalService
{
    private readonly IFestivalRepository _festivalRepository;

    public FestivalService(IFestivalRepository festivalRepository)
    {
        _festivalRepository = festivalRepository;
    }

    public async Task<(IReadOnlyList<Festival> Festivals, string SearchQuery)> GetIndexDataAsync(string? query)
    {
        var normalized = query?.Trim() ?? string.Empty;
        var festivals = await _festivalRepository.GetFestivalsAsync(normalized);
        return (festivals, normalized);
    }

    public async Task CreateAsync(FestivalDto festivalDto)
    {
        var festival = new Festival
        {
            Title = festivalDto.Title,
            Description = festivalDto.Description,
            Location = festivalDto.Location,
            City = festivalDto.City,
            Country = festivalDto.Country,
            Status = festivalDto.Status,
            StartDate = festivalDto.StartDate,
            EndDate = festivalDto.EndDate
        };

        _festivalRepository.AddFestival(festival);
        await _festivalRepository.SaveChangesAsync();
    }

    public async Task<FestivalDetailsDto?> GetEditDtoAsync(Guid id)
    {
        var festival = await _festivalRepository.GetFestivalByIdWithDetailsAsync(id);
        if (festival == null)
        {
            return null;
        }

        var festivalDto = new FestivalDetailsDto
        {
            Id = festival.Id,
            Title = festival.Title,
            Description = festival.Description,
            Location = festival.Location,
            City = festival.City,
            Country = festival.Country,
            Status = festival.Status,
            StartDate = festival.StartDate,
            EndDate = festival.EndDate,
            ExistingPictureUrl = festival.PictureUrl,
            Lineups = festival.Lineups
                .OrderBy(l => l.CreatedAt)
                .Select(l => new FestivalLineupEditDto
                {
                    Id = l.Id,
                    ArtistName = l.Artist.Name,
                    SlotDetails = l.Description,
                    PictureUrl = l.Artist.PictureUrl
                })
                .ToList(),
            TicketCategories = festival.TicketCategories
                .OrderByDescending(t => t.Price)
                .Select(t => new FestivalTicketTierEditDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Price = t.Price,
                    Stock = t.Stock
                })
                .ToList()
        };

        await PopulateStatsAsync(festivalDto, id);
        return festivalDto;
    }

    public async Task PopulateStatsAsync(FestivalDetailsDto festivalDto, Guid festivalId)
    {
        festivalDto.SoldTickets = await _festivalRepository.CountSoldTicketsAsync(festivalId);
        festivalDto.LiveRevenueRon = await _festivalRepository.SumLiveRevenueAsync(festivalId);
        festivalDto.TotalTicketCapacity = await _festivalRepository.SumTicketCapacityAsync(festivalId);
    }

    public async Task<FestivalEditResult> EditAsync(Guid id, FestivalDetailsDto festivalDto)
    {
        var validationErrors = ValidateEditPayload(festivalDto);
        if (validationErrors.Count > 0)
        {
            await PopulateStatsAsync(festivalDto, id);
            return new FestivalEditResult
            {
                IsSuccess = false,
                ValidationErrors = validationErrors
            };
        }

        var festival = await _festivalRepository.GetFestivalByIdWithDetailsAsync(id);
        if (festival == null)
        {
            return new FestivalEditResult { IsNotFound = true };
        }

        festival.Title = festivalDto.Title;
        festival.Description = festivalDto.Description;
        festival.Location = festivalDto.Location;
        festival.City = festivalDto.City;
        festival.Country = festivalDto.Country;
        festival.Status = festivalDto.Status;
        festival.StartDate = festivalDto.StartDate;
        festival.EndDate = festivalDto.EndDate;

        await SyncLineupAsync(festival, festivalDto.Lineups);
        SyncTicketCategories(festival, festivalDto.TicketCategories);

        var canSave = await _festivalRepository.RefreshOrDetachStaleEntriesAsync();
        if (!canSave)
        {
            await PopulateStatsAsync(festivalDto, id);
            return new FestivalEditResult
            {
                ErrorMessage = "Festival no longer exists. Please reload the page."
            };
        }

        try
        {
            await _festivalRepository.SaveChangesAsync();
            return new FestivalEditResult { IsSuccess = true };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var resolved = await _festivalRepository.ResolveConcurrencyConflictsAsync(ex);
            if (resolved)
            {
                try
                {
                    await _festivalRepository.SaveChangesAsync();
                    return new FestivalEditResult { IsSuccess = true };
                }
                catch (DbUpdateConcurrencyException retryEx)
                {
                    var retryTypes = string.Join(", ", retryEx.Entries.Select(e => e.Metadata.ClrType.Name).Distinct());
                    await PopulateStatsAsync(festivalDto, id);
                    return new FestivalEditResult
                    {
                        ErrorMessage = $"Could not save due to repeated parallel changes ({retryTypes}). Please reload and try again."
                    };
                }
            }

            var conflictingTypes = string.Join(", ", ex.Entries.Select(e => e.Metadata.ClrType.Name).Distinct());
            await PopulateStatsAsync(festivalDto, id);
            return new FestivalEditResult
            {
                ErrorMessage = $"Could not save due to parallel changes ({conflictingTypes}). Please reload and try again."
            };
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var festival = await _festivalRepository.FindFestivalByIdAsync(id);
        if (festival == null)
        {
            return;
        }

        _festivalRepository.RemoveFestival(festival);
        await _festivalRepository.SaveChangesAsync();
    }

    private static Dictionary<string, string> ValidateEditPayload(FestivalDetailsDto festivalDto)
    {
        var errors = new Dictionary<string, string>();

        if (festivalDto.StartDate > festivalDto.EndDate)
        {
            errors[nameof(FestivalDetailsDto.EndDate)] = "End date must be after start date.";
        }

        var activeTicketTiers = festivalDto.TicketCategories
            .Select((tier, index) => new { Tier = tier, Index = index })
            .Where(x => !x.Tier.IsDeleted)
            .Where(x => !string.IsNullOrWhiteSpace(x.Tier.Name))
            .ToList();

        foreach (var item in activeTicketTiers)
        {
            if (item.Tier.Price <= 0)
            {
                errors[$"TicketCategories[{item.Index}].Price"] = "Price must be greater than 0.";
            }

            if (item.Tier.Stock < 0)
            {
                errors[$"TicketCategories[{item.Index}].Stock"] = "Stock cannot be negative.";
            }
        }

        return errors;
    }

    private async Task SyncLineupAsync(Festival festival, IEnumerable<FestivalLineupEditDto> lineups)
    {
        var existingById = festival.Lineups.ToDictionary(l => l.Id);
        var processedIds = new HashSet<Guid>();

        foreach (var lineupDto in lineups)
        {
            var hasArtist = !string.IsNullOrWhiteSpace(lineupDto.ArtistName);

            if (lineupDto.Id.HasValue && existingById.TryGetValue(lineupDto.Id.Value, out var existing))
            {
                if (!processedIds.Add(lineupDto.Id.Value))
                {
                    continue;
                }

                if (lineupDto.IsDeleted || !hasArtist)
                {
                    _festivalRepository.RemoveLineup(existing);
                    continue;
                }

                var artist = await GetOrCreateArtistAsync(lineupDto.ArtistName, lineupDto.PictureUrl);
                existing.Artist = artist;
                existing.ArtistId = artist.Id;
                existing.Description = lineupDto.SlotDetails?.Trim();
                existing.StartDate = festival.StartDate;
                existing.EndDate = festival.EndDate;
                continue;
            }

            if (lineupDto.IsDeleted || !hasArtist)
            {
                continue;
            }

            var newArtist = await GetOrCreateArtistAsync(lineupDto.ArtistName, lineupDto.PictureUrl);
            _festivalRepository.AddLineup(new Lineup
            {
                FestivalId = festival.Id,
                Festival = festival,
                ArtistId = newArtist.Id,
                Artist = newArtist,
                Description = lineupDto.SlotDetails?.Trim(),
                StartDate = festival.StartDate,
                EndDate = festival.EndDate
            });
        }
    }

    private void SyncTicketCategories(Festival festival, IEnumerable<FestivalTicketTierEditDto> tiers)
    {
        var existingById = festival.TicketCategories.ToDictionary(t => t.Id);
        var processedIds = new HashSet<Guid>();

        foreach (var tierDto in tiers)
        {
            var hasName = !string.IsNullOrWhiteSpace(tierDto.Name);

            if (tierDto.Id.HasValue && existingById.TryGetValue(tierDto.Id.Value, out var existing))
            {
                if (!processedIds.Add(tierDto.Id.Value))
                {
                    continue;
                }

                if (tierDto.IsDeleted || !hasName)
                {
                    _festivalRepository.RemoveTicketCategory(existing);
                    continue;
                }

                existing.Name = tierDto.Name.Trim();
                existing.Description = tierDto.Description?.Trim();
                existing.Price = Math.Round(Math.Max(tierDto.Price, 0), 2);
                existing.Stock = Math.Max(tierDto.Stock, 0);
                continue;
            }

            if (tierDto.IsDeleted || !hasName)
            {
                continue;
            }

            _festivalRepository.AddTicketCategory(new TicketCategory
            {
                FestivalId = festival.Id,
                Festival = festival,
                Name = tierDto.Name.Trim(),
                Description = tierDto.Description?.Trim(),
                Price = Math.Round(Math.Max(tierDto.Price, 0), 2),
                Stock = Math.Max(tierDto.Stock, 0)
            });
        }
    }

    private async Task<Artist> GetOrCreateArtistAsync(string name, string? pictureUrl)
    {
        var normalizedName = name.Trim();
        var normalizedLower = normalizedName.ToLower();

        var artist = await _festivalRepository.FindArtistByNameAsync(normalizedLower);
        if (artist != null)
        {
            if (string.IsNullOrWhiteSpace(artist.PictureUrl) && !string.IsNullOrWhiteSpace(pictureUrl))
            {
                artist.PictureUrl = pictureUrl.Trim();
            }

            return artist;
        }

        artist = new Artist
        {
            Name = normalizedName,
            PictureUrl = string.IsNullOrWhiteSpace(pictureUrl) ? null : pictureUrl.Trim()
        };

        _festivalRepository.AddArtist(artist);
        return artist;
    }
}

