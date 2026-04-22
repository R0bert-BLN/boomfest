using BoomFest.Data;
using BoomFest.Enums;
using BoomFest.Models;
using Microsoft.EntityFrameworkCore;

namespace BoomFest.Repositories;

public class FestivalRepository : IFestivalRepository
{
    private readonly ApplicationDbContext _context;

    public FestivalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Festival>> GetFestivalsAsync(string? search)
    {
        var query = _context.Festivals
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            query = query.Where(f =>
                f.Title.Contains(normalized) ||
                f.Location.Contains(normalized) ||
                f.City.Contains(normalized) ||
                f.Country.Contains(normalized));
        }

        return await query
            .OrderBy(f => f.StartDate)
            .ToListAsync();
    }

    public Task<Festival?> GetFestivalByIdWithDetailsAsync(Guid id)
    {
        return _context.Festivals
            .Include(f => f.Lineups)
            .ThenInclude(l => l.Artist)
            .Include(f => f.TicketCategories)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public Task<Festival?> FindFestivalByIdAsync(Guid id)
    {
        return _context.Festivals.FindAsync(id).AsTask();
    }

    public void AddFestival(Festival festival)
    {
        _context.Festivals.Add(festival);
    }

    public void RemoveFestival(Festival festival)
    {
        _context.Festivals.Remove(festival);
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public Task<int> CountSoldTicketsAsync(Guid festivalId)
    {
        return _context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.Category.FestivalId == festivalId &&
                             (t.Status == TicketStatus.Sold || t.Status == TicketStatus.Used));
    }

    public async Task<decimal> SumLiveRevenueAsync(Guid festivalId)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Category.FestivalId == festivalId &&
                        (t.Status == TicketStatus.Sold || t.Status == TicketStatus.Used))
            .SumAsync(t => (decimal?)t.Category.Price) ?? 0m;
    }

    public async Task<int> SumTicketCapacityAsync(Guid festivalId)
    {
        return await _context.TicketCategories
            .AsNoTracking()
            .Where(c => c.FestivalId == festivalId)
            .SumAsync(c => (int?)c.Stock) ?? 0;
    }

    public Task<Artist?> FindArtistByNameAsync(string normalizedLowerName)
    {
        return _context.Artists
            .FirstOrDefaultAsync(a => a.Name.ToLower() == normalizedLowerName);
    }

    public void AddArtist(Artist artist)
    {
        _context.Artists.Add(artist);
    }

    public void AddLineup(Lineup lineup)
    {
        _context.Lineups.Add(lineup);
    }

    public void RemoveLineup(Lineup lineup)
    {
        _context.Lineups.Remove(lineup);
    }

    public void AddTicketCategory(TicketCategory ticketCategory)
    {
        _context.TicketCategories.Add(ticketCategory);
    }

    public void RemoveTicketCategory(TicketCategory ticketCategory)
    {
        _context.TicketCategories.Remove(ticketCategory);
    }

    public async Task<bool> ResolveConcurrencyConflictsAsync(DbUpdateConcurrencyException exception)
    {
        foreach (var entry in exception.Entries)
        {
            if (entry.State == EntityState.Added)
            {
                // Keep newly-added entities pending so they can still be inserted on retry.
                continue;
            }

            var databaseValues = await entry.GetDatabaseValuesAsync();
            if (databaseValues == null)
            {
                if (entry.Entity is Festival)
                {
                    return false;
                }

                entry.State = EntityState.Detached;
                continue;
            }

            entry.OriginalValues.SetValues(databaseValues);
        }

        return true;
    }

    public async Task<bool> RefreshOrDetachStaleEntriesAsync()
    {
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            var databaseValues = await entry.GetDatabaseValuesAsync();
            if (databaseValues == null)
            {
                if (entry.Entity is Festival)
                {
                    return false;
                }

                entry.State = EntityState.Detached;
                continue;
            }

            entry.OriginalValues.SetValues(databaseValues);
        }

        return true;
    }
}

