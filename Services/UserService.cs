using BoomFest.Dtos;
using BoomFest.Models;
using BoomFest.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BoomFest.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<(IReadOnlyList<User> Users, string SearchQuery)> GetIndexDataAsync(string? query)
    {
        var normalized = query?.Trim() ?? string.Empty;
        var users = await _userRepository.GetUsersAsync(normalized);
        return (users, normalized);
    }

    public async Task<UserOperationResult> CreateAsync(UserDto userDto)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(userDto.Password))
        {
            errors[nameof(UserDto.Password)] = "Password is required";
        }

        var email = userDto.Email.Trim();
        if (await _userRepository.EmailExistsAsync(email))
        {
            errors[nameof(UserDto.Email)] = "Email is already in use";
        }

        if (errors.Count > 0)
        {
            return new UserOperationResult { ValidationErrors = errors };
        }

        var user = new User
        {
            FirstName = userDto.FirstName.Trim(),
            LastName = userDto.LastName.Trim(),
            Email = email,
            Password = string.Empty,
            Role = userDto.Role
        };

        user.Password = _passwordHasher.HashPassword(user, userDto.Password!);

        _userRepository.Add(user);
        await _userRepository.SaveChangesAsync();

        return new UserOperationResult { IsSuccess = true };
    }

    public async Task<UserDto?> GetEditDtoAsync(Guid id)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<UserOperationResult> EditAsync(Guid id, UserDto userDto)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user == null)
        {
            return new UserOperationResult { IsNotFound = true };
        }

        var email = userDto.Email.Trim();
        var errors = new Dictionary<string, string>();

        if (await _userRepository.EmailExistsAsync(email, id))
        {
            errors[nameof(UserDto.Email)] = "Email is already in use";
        }

        if (errors.Count > 0)
        {
            return new UserOperationResult { ValidationErrors = errors };
        }

        user.FirstName = userDto.FirstName.Trim();
        user.LastName = userDto.LastName.Trim();
        user.Email = email;
        user.Role = userDto.Role;

        if (!string.IsNullOrWhiteSpace(userDto.Password))
        {
            user.Password = _passwordHasher.HashPassword(user, userDto.Password);
        }

        await _userRepository.SaveChangesAsync();

        return new UserOperationResult { IsSuccess = true };
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user == null)
        {
            return;
        }

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();
    }
}
