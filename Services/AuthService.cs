using System;
using BoomFest.Dtos;
using BoomFest.Enums;
using BoomFest.Models;
using BoomFest.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BoomFest.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResult> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return new AuthResult { ErrorMessage = "Invalid email or password." };
        }

        PasswordVerificationResult verification;
        try
        {
            verification = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
        }
        catch (FormatException)
        {
            verification = PasswordVerificationResult.Failed;
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = _passwordHasher.HashPassword(user, loginDto.Password);
            await _userRepository.SaveChangesAsync();
        }
        else if (verification == PasswordVerificationResult.Failed)
        {
            // Support legacy plaintext passwords and upgrade to hashed.
            if (!string.Equals(user.Password, loginDto.Password, StringComparison.Ordinal))
            {
                return new AuthResult { ErrorMessage = "Invalid email or password." };
            }

            user.Password = _passwordHasher.HashPassword(user, loginDto.Password);
            await _userRepository.SaveChangesAsync();
        }

        return new AuthResult { IsSuccess = true, User = user };
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto registerDto)
    {
        var email = registerDto.Email.Trim();
        if (await _userRepository.EmailExistsAsync(email))
        {
            return new AuthResult { ErrorMessage = "Email is already in use." };
        }

        var user = new User
        {
            FirstName = registerDto.FirstName.Trim(),
            LastName = registerDto.LastName.Trim(),
            Email = email,
            Role = UserRole.User,
            VerifiedAt = DateTime.UtcNow,
            Password = string.Empty
        };

        user.Password = _passwordHasher.HashPassword(user, registerDto.Password);

        _userRepository.Add(user);
        await _userRepository.SaveChangesAsync();

        return new AuthResult { IsSuccess = true, User = user };
    }
}
