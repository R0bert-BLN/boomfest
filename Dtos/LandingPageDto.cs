using System;
using System.Collections.Generic;

namespace BoomFest.Dtos;

public class LandingPageDto
{
    public IReadOnlyList<FestivalCardDto> Festivals { get; init; } = Array.Empty<FestivalCardDto>();
}

public class FestivalCardDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public string? PictureUrl { get; init; }
    public decimal? MinPrice { get; init; }
}

