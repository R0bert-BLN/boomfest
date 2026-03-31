using BoomFest.Enums;

namespace BoomFest.Models;

public class Order : Model
{
    public required Guid UserId { get; set; }
    public required string StripeSessionId { get; set; }
    public required decimal TotalPrice { get; set; }
    public required OrderStatus Status { get; set; }

    public required User User { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
