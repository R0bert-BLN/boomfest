using BoomFest.Data;
using BoomFest.Dtos;
using BoomFest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoomFest.Controllers;

[Route("Admin/[controller]")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var usersQuery = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim();
            usersQuery = usersQuery.Where(user =>
                user.FirstName.Contains(search) ||
                user.LastName.Contains(search) ||
                user.Email.Contains(search));
        }

        var users = await usersQuery
            .OrderByDescending(user => user.CreatedAt)
            .ToListAsync();

        ViewData["SearchQuery"] = q;
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
        if (string.IsNullOrWhiteSpace(userDto.Password))
        {
            ModelState.AddModelError(nameof(UserDto.Password), "Password is required");
        }

        if (await _context.Users.AnyAsync(user => user.Email == userDto.Email))
        {
            ModelState.AddModelError(nameof(UserDto.Email), "Email is already in use");
        }

        if (!ModelState.IsValid)
        {
            return View(userDto);
        }

        var user = new User
        {
            FirstName = userDto.FirstName.Trim(),
            LastName = userDto.LastName.Trim(),
            Email = userDto.Email.Trim(),
            Password = userDto.Password!,
            Role = userDto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        };

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

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (await _context.Users.AnyAsync(existingUser => existingUser.Id != id && existingUser.Email == userDto.Email))
        {
            ModelState.AddModelError(nameof(UserDto.Email), "Email is already in use");
        }

        if (!ModelState.IsValid)
        {
            return View(userDto);
        }

        user.FirstName = userDto.FirstName.Trim();
        user.LastName = userDto.LastName.Trim();
        user.Email = userDto.Email.Trim();
        user.Role = userDto.Role;

        if (!string.IsNullOrWhiteSpace(userDto.Password))
        {
            user.Password = userDto.Password;
        }

        await _context.SaveChangesAsync();

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
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}

