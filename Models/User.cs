using BoomFest.Enums;

namespace BoomFest.Models;

public class User : Model
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required UserRole Role { get; set; }
    public DateTime? VerifiedAt { get; set; }
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
