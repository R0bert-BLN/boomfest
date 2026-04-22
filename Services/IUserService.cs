using BoomFest.Dtos;
using BoomFest.Models;

namespace BoomFest.Services;

public interface IUserService
{
    Task<(IReadOnlyList<User> Users, string SearchQuery)> GetIndexDataAsync(string? query);
    Task<UserOperationResult> CreateAsync(UserDto userDto);
    Task<UserDto?> GetEditDtoAsync(Guid id);
    Task<UserOperationResult> EditAsync(Guid id, UserDto userDto);
    Task DeleteAsync(Guid id);
}

public class UserOperationResult
{
    public bool IsSuccess { get; init; }
    public bool IsNotFound { get; init; }
    public Dictionary<string, string> ValidationErrors { get; init; } = new();
}

