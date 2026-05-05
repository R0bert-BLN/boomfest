using BoomFest.Dtos;
using BoomFest.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoomFest.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("/Cart")]
    public async Task<IActionResult> Index()
    {
        var model = await _cartService.GetCartAsync();
        return View(model);
    }

    [HttpPost("/Cart/Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddToCartDto request)
    {
        if (request.FestivalId == Guid.Empty)
        {
            return RedirectToAction("Index", "Events");
        }

        await _cartService.AddToCartAsync(request.FestivalId, request.Quantities ?? new List<CartQuantityDto>());
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/Cart/Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Dictionary<Guid, int> quantities)
    {
        await _cartService.UpdateQuantitiesAsync(quantities);
        return RedirectToAction(nameof(Index));
    }
}
