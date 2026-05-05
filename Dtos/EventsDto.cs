using System;
using System.Collections.Generic;

namespace BoomFest.Dtos;

public class EventsIndexDto
{
    public string SearchQuery { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public IReadOnlyList<EventCardDto> Events { get; init; } = Array.Empty<EventCardDto>();
}

public class EventCardDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public string? PictureUrl { get; init; }
    public decimal? MinPrice { get; init; }
    public bool IsSoldOut { get; init; }
}

public class EventDetailsDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string? PictureUrl { get; init; }
    public bool IsSoldOut { get; init; }
    public IReadOnlyList<EventLineupDto> Lineups { get; init; } = Array.Empty<EventLineupDto>();
    public IReadOnlyList<EventTicketTierDto> TicketTiers { get; init; } = Array.Empty<EventTicketTierDto>();
}

public class EventLineupDto
{
    public string ArtistName { get; init; } = string.Empty;
    public string? ArtistImageUrl { get; init; }
    public string? Description { get; init; }
}

public class EventTicketTierDto
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Remaining { get; init; }
}

