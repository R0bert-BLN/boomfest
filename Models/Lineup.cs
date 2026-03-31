namespace BoomFest.Models;

public class Lineup : Model
{
    public Guid FestivalId { get; set; }
    public Guid ArtistId { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public required Festival Festival { get; set; }
    public required Artist Artist { get; set; }
}
