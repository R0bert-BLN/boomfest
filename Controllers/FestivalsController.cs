using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoomFest.Data;
using BoomFest.Dtos;
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
    public async Task<IActionResult> Index()
    {
        var festivals = await _context.Festivals
            .AsNoTracking()
            .ToListAsync();

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
        var festival = await _context.Festivals.FindAsync(id);

        if (festival == null)
        {
            return NotFound();
        }

        var festivalDto = new FestivalDto
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
            ExistingPictureUrl = festival.PictureUrl
        };

        return View(festivalDto);
    }

    [HttpPost("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, FestivalDto festivalDto)
    {
        if (id != festivalDto.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(festivalDto);
        }

        var festival = await _context.Festivals.FindAsync(id);

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

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
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

    private bool FestivalExists(Guid id) => _context.Festivals.Any(e => e.Id == id);
}
