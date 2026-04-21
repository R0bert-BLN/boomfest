using Microsoft.AspNetCore.Mvc;

namespace BoomFest.Controllers;

[Route("Admin/[controller]")]
public class TransactionsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}

