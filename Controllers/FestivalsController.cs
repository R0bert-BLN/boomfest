using BoomFest.Dtos;
using BoomFest.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoomFest.Controllers;

[Route("Admin/[controller]")]
public class FestivalsController : Controller
{
    private readonly IFestivalService _festivalService;

    public FestivalsController(IFestivalService festivalService)
    {
        _festivalService = festivalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var (festivals, searchQuery) = await _festivalService.GetIndexDataAsync(q);
        ViewData["SearchQuery"] = searchQuery;
        return View(festivals);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new FestivalDto());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FestivalDto festivalDto)
    {
        if (!ModelState.IsValid)
        {
            return View(festivalDto);
        }

        await _festivalService.CreateAsync(festivalDto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var festivalDto = await _festivalService.GetEditDtoAsync(id);
        if (festivalDto == null)
        {
            return NotFound();
        }

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

        if (!ModelState.IsValid)
        {
            await _festivalService.PopulateStatsAsync(festivalDto, id);
            return View("Edit", festivalDto);
        }

        var result = await _festivalService.EditAsync(id, festivalDto);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        foreach (var validationError in result.ValidationErrors)
        {
            ModelState.AddModelError(validationError.Key, validationError.Value);
        }

        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
        }

        if (!result.IsSuccess)
        {
            return View("Edit", festivalDto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _festivalService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
