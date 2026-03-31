using BoomFest.Enums;

namespace BoomFest.Models;

public class Ticket : Model
{
    public required Guid CategoryId { get; set; }
    public required Guid OrderId { get; set; } 
    public Guid QrCode { get; set; } = Guid.NewGuid();
    public TicketStatus Status { get; set; } = TicketStatus.Reserved;
    public DateTime? ScannedAt { get; set; }
    public Guid? ScannedBy { get; set; }

    public required TicketCategory Category { get; set; }
    public required Order Order { get; set; }
    public User? Scanner { get; set; }
}
