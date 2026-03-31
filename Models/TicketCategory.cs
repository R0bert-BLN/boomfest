namespace BoomFest.Models;

public class TicketCategory : Model
{
    public required Guid FestivalId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required decimal Price { get; set; } 
    public required int Stock { get; set; }

    public required Festival Festival { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
