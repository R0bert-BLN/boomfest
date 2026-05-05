using BoomFest.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoomFest.Controllers;

public class HomeController : Controller
{
    private readonly IFestivalService _festivalService;

    public HomeController(IFestivalService festivalService)
    {
        _festivalService = festivalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var landingPage = await _festivalService.GetLandingPageAsync();
        return View(landingPage);
    }
}

