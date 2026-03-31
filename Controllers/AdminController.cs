using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BoomFest.Models;

namespace BoomFest.Controllers;

[Route("[controller]")]
public class AdminController : Controller
{
    [HttpGet("Dashboard")]
    public IActionResult Dashboard()
    {
        return View();
    }
}
