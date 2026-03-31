namespace BoomFest.Models;

public class Artist : Model
{
    public required string Name { get; set; }
    public string? PictureUrl { get; set; }
    public string? Description { get; set; }
}
