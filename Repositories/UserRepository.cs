using BoomFest.Data;
using BoomFest.Models;
using Microsoft.EntityFrameworkCore;

namespace BoomFest.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetUsersAsync(string? search)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            query = query.Where(user =>
                user.FirstName.Contains(normalized) ||
                user.LastName.Contains(normalized) ||
                user.Email.Contains(normalized));
        }

        return await query
            .OrderByDescending(user => user.CreatedAt)
            .ToListAsync();
    }

    public Task<User?> FindByIdAsync(Guid id)
    {
        return _context.Users.FindAsync(id).AsTask();
    }

    public Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
    {
        if (excludeUserId.HasValue)
        {
            return _context.Users.AnyAsync(user => user.Email == email && user.Id != excludeUserId.Value);
        }

        return _context.Users.AnyAsync(user => user.Email == email);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }

    public void Remove(User user)
    {
        _context.Users.Remove(user);
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}

