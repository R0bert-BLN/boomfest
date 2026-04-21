using System.ComponentModel.DataAnnotations;
using BoomFest.Enums;

namespace BoomFest.Dtos;

public class UserDto
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; set; } = null!;

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }
}

