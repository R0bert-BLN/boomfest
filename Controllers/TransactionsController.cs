using BoomFest.Dtos;
using BoomFest.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoomFest.Controllers;

[Route("Admin/[controller]")]
public class TransactionsController : Controller
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new TransactionsIndexDto
        {
            TotalOrders = await _transactionService.GetTotalOrdersAsync()
        };

        return View(model);
    }
}
