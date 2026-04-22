using BoomFest.Models;
using Microsoft.EntityFrameworkCore;

namespace BoomFest.Repositories;

public interface IFestivalRepository
{
    Task<List<Festival>> GetFestivalsAsync(string? search);
    Task<Festival?> GetFestivalByIdWithDetailsAsync(Guid id);
    Task<Festival?> FindFestivalByIdAsync(Guid id);
    Task<int> SaveChangesAsync();

    Task<int> CountSoldTicketsAsync(Guid festivalId);
    Task<decimal> SumLiveRevenueAsync(Guid festivalId);
    Task<int> SumTicketCapacityAsync(Guid festivalId);

    Task<Artist?> FindArtistByNameAsync(string normalizedLowerName);
    void AddArtist(Artist artist);

    void AddFestival(Festival festival);
    void RemoveFestival(Festival festival);

    void AddLineup(Lineup lineup);
    void RemoveLineup(Lineup lineup);

    void AddTicketCategory(TicketCategory ticketCategory);
    void RemoveTicketCategory(TicketCategory ticketCategory);

    Task<bool> ResolveConcurrencyConflictsAsync(DbUpdateConcurrencyException exception);
    Task<bool> RefreshOrDetachStaleEntriesAsync();
}

