using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoomFest.Data;
using BoomFest.Dtos;
using BoomFest.Enums;
using BoomFest.Models;

namespace BoomFest.Controllers;

[Route("Admin/[controller]")]
public class FestivalsController : Controller
{
    private readonly ApplicationDbContext _context;

    public FestivalsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var festivalsQuery = _context.Festivals
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim();
            festivalsQuery = festivalsQuery.Where(f =>
                f.Title.Contains(search) ||
                f.Location.Contains(search) ||
                f.City.Contains(search) ||
                f.Country.Contains(search));
        }

        var festivals = await festivalsQuery
            .OrderBy(f => f.StartDate)
            .ToListAsync();

        ViewData["SearchQuery"] = q?.Trim() ?? string.Empty;

        return View(festivals);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new FestivalDto());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(FestivalDto festivalDto)
    {
        if (!ModelState.IsValid)
        {
            return View(festivalDto);
        }

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


        _context.Festivals.Add(festival);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var festival = await _context.Festivals
            .Include(f => f.Lineups)
            .ThenInclude(l => l.Artist)
            .Include(f => f.TicketCategories)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (festival == null)
        {
            return NotFound();
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

        await PopulateFestivalStatsAsync(festivalDto, id);

        return View("Edit", festivalDto);
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, FestivalDetailsDto festivalDto)
    {
        if (id != festivalDto.Id)
        {
            return NotFound();
        }

        NormalizeCollections(festivalDto);
        ValidateEditPayload(festivalDto);

        if (!ModelState.IsValid)
        {
            await PopulateFestivalStatsAsync(festivalDto, id);
            return View("Edit", festivalDto);
        }

        var festival = await _context.Festivals
            .Include(f => f.Lineups)
            .ThenInclude(l => l.Artist)
            .Include(f => f.TicketCategories)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (festival == null)
        {
            return NotFound();
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

        var canSave = await RefreshOrDetachStaleEntriesAsync();
        if (!canSave)
        {
            ModelState.AddModelError(string.Empty, "Festival no longer exists. Please reload the page.");
            await PopulateFestivalStatsAsync(festivalDto, id);
            return View("Edit", festivalDto);
        }

        try
        {
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var resolved = await ResolveConcurrencyConflictsAsync(ex);
            if (resolved)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException retryEx)
                {
                    var retryTypes = string.Join(", ", retryEx.Entries.Select(e => e.Metadata.ClrType.Name).Distinct());
                    ModelState.AddModelError(string.Empty, $"Could not save due to repeated parallel changes ({retryTypes}). Please reload and try again.");
                    await PopulateFestivalStatsAsync(festivalDto, id);
                    return View("Edit", festivalDto);
                }
            }

            var conflictingTypes = string.Join(", ", ex.Entries.Select(e => e.Metadata.ClrType.Name).Distinct());
            ModelState.AddModelError(string.Empty, $"Could not save due to parallel changes ({conflictingTypes}). Please reload and try again.");
            await PopulateFestivalStatsAsync(festivalDto, id);
            return View("Edit", festivalDto);
        }
    }

    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var festival = await _context.Festivals.FindAsync(id);
        if (festival != null)
        {
            _context.Festivals.Remove(festival);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private void NormalizeCollections(FestivalDetailsDto festivalDto)
    {
        // Collection properties are initialized in DTO defaults; this keeps a single normalization hook.
    }

    private void ValidateEditPayload(FestivalDetailsDto festivalDto)
    {
        if (festivalDto.StartDate > festivalDto.EndDate)
        {
            ModelState.AddModelError(nameof(FestivalDetailsDto.EndDate), "End date must be after start date.");
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
                ModelState.AddModelError($"TicketCategories[{item.Index}].Price", "Price must be greater than 0.");
            }

            if (item.Tier.Stock < 0)
            {
                ModelState.AddModelError($"TicketCategories[{item.Index}].Stock", "Stock cannot be negative.");
            }
        }
    }

    private async Task PopulateFestivalStatsAsync(FestivalDetailsDto festivalDto, Guid festivalId)
    {
        festivalDto.SoldTickets = await _context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.Category.FestivalId == festivalId &&
                             (t.Status == TicketStatus.Sold || t.Status == TicketStatus.Used));

        festivalDto.LiveRevenueRon = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Category.FestivalId == festivalId &&
                        (t.Status == TicketStatus.Sold || t.Status == TicketStatus.Used))
            .SumAsync(t => (decimal?)t.Category.Price) ?? 0m;

        festivalDto.TotalTicketCapacity = await _context.TicketCategories
            .AsNoTracking()
            .Where(c => c.FestivalId == festivalId)
            .SumAsync(c => (int?)c.Stock) ?? 0;
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
                    _context.Lineups.Remove(existing);
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
            _context.Lineups.Add(new Lineup
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
                    _context.TicketCategories.Remove(existing);
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

            _context.TicketCategories.Add(new TicketCategory
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

        var artist = await _context.Artists
            .FirstOrDefaultAsync(a => a.Name.ToLower() == normalizedLower);

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

        _context.Artists.Add(artist);
        return artist;
    }

    private static async Task<bool> ResolveConcurrencyConflictsAsync(DbUpdateConcurrencyException exception)
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

                // Entity was already removed by another operation. Ignore stale updates/deletes.
                entry.State = EntityState.Detached;
                continue;
            }

            entry.OriginalValues.SetValues(databaseValues);
        }

        return true;
    }

    private async Task<bool> RefreshOrDetachStaleEntriesAsync()
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
