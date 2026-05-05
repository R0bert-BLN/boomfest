using BoomFest.Dtos;
using BoomFest.Models;

namespace BoomFest.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginDto loginDto);
    Task<AuthResult> RegisterAsync(RegisterDto registerDto);
}

public class AuthResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public User? User { get; init; }
}

