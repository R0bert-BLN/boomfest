using BoomFest.Models;

namespace BoomFest.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetUsersAsync(string? search);
    Task<User?> FindByIdAsync(Guid id);
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
    void Add(User user);
    void Remove(User user);
    Task<int> SaveChangesAsync();
}

