using BoomFest.Dtos;
using BoomFest.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoomFest.Controllers;

[Route("Admin/[controller]")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var (users, searchQuery) = await _userService.GetIndexDataAsync(q);
        ViewData["SearchQuery"] = searchQuery;
        return View(users);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new UserDto());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return View(userDto);
        }

        var result = await _userService.CreateAsync(userDto);
        foreach (var validationError in result.ValidationErrors)
        {
            ModelState.AddModelError(validationError.Key, validationError.Value);
        }

        if (!result.IsSuccess)
        {
            return View(userDto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userDto = await _userService.GetEditDtoAsync(id);
        if (userDto == null)
        {
            return NotFound();
        }

        return View(userDto);
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UserDto userDto)
    {
        if (id != userDto.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(userDto);
        }

        var result = await _userService.EditAsync(id, userDto);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        foreach (var validationError in result.ValidationErrors)
        {
            ModelState.AddModelError(validationError.Key, validationError.Value);
        }

        if (!result.IsSuccess)
        {
            return View(userDto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("View/{id:guid}")]
    public IActionResult Details(Guid id)
    {
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}

