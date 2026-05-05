using BoomFest.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoomFest.Controllers;

public class EventsController : Controller
{
    private readonly IEventsService _eventsService;

    public EventsController(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    [HttpGet("/Events")]
    public async Task<IActionResult> Index(string? q)
    {
        var model = await _eventsService.GetEventsAsync(q);
        return View(model);
    }

    [HttpGet("/Events/{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var model = await _eventsService.GetEventDetailsAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }
}

