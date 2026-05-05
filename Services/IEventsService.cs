using BoomFest.Dtos;

namespace BoomFest.Services;

public interface IEventsService
{
    Task<EventsIndexDto> GetEventsAsync(string? query);
    Task<EventDetailsDto?> GetEventDetailsAsync(Guid id);
}

