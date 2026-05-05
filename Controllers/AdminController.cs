using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BoomFest.Models;
using Microsoft.AspNetCore.Authorization;
using BoomFest.Dtos;
using BoomFest.Services;

namespace BoomFest.Controllers;

[Authorize(Roles = "Admin,Staff")]
[Route("[controller]")]
public class AdminController : Controller
{
    private readonly IFestivalService _festivalService;
    private readonly IUserService _userService;
    private readonly ITransactionService _transactionService;

    public AdminController(IFestivalService festivalService, IUserService userService, ITransactionService transactionService)
    {
        _festivalService = festivalService;
        _userService = userService;
        _transactionService = transactionService;
    }

    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var (festivals, _) = await _festivalService.GetIndexDataAsync(null);
        var (users, _) = await _userService.GetIndexDataAsync(null);
        var totalOrders = await _transactionService.GetTotalOrdersAsync();

        var now = DateTime.UtcNow;
        var stats = new DashboardStatsDto
        {
            TotalFestivals = festivals.Count,
            PublishedFestivals = festivals.Count(f => f.Status == BoomFest.Enums.FestivalStatus.Published),
            UpcomingFestivals = festivals.Count(f => f.StartDate.ToUniversalTime() > now),
            TotalUsers = users.Count,
            TotalOrders = totalOrders
        };

        return View(stats);
    }
}
