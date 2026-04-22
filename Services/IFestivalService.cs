using BoomFest.Dtos;
using BoomFest.Models;

namespace BoomFest.Services;

public interface IFestivalService
{
    Task<(IReadOnlyList<Festival> Festivals, string SearchQuery)> GetIndexDataAsync(string? query);
    Task CreateAsync(FestivalDto festivalDto);
    Task<FestivalDetailsDto?> GetEditDtoAsync(Guid id);
    Task PopulateStatsAsync(FestivalDetailsDto festivalDto, Guid festivalId);
    Task<FestivalEditResult> EditAsync(Guid id, FestivalDetailsDto festivalDto);
    Task DeleteAsync(Guid id);
}

public class FestivalEditResult
{
    public bool IsSuccess { get; init; }
    public bool IsNotFound { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, string> ValidationErrors { get; init; } = new();
}

