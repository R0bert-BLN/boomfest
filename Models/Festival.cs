using BoomFest.Enums;

namespace BoomFest.Models;

public class Festival : Model
{
    public required string Title { get; set; }
    public string? PictureUrl { get; set; }
    public string? Description { get; set; }
    public required FestivalStatus Status { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public required string Location { get; set; }
    
    public ICollection<TicketCategory> TicketCategories { get; set; } = new List<TicketCategory>();
    public ICollection<Lineup> Lineups { get; set; } = new List<Lineup>();
}
