using System.ComponentModel.DataAnnotations;
using BoomFest.Enums;
using BoomFest.Models;

namespace BoomFest.Dtos;

public class FestivalDto
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Location is required")]
    public string Location { get; set; } = null!;

    [Required]
    public string City { get; set; } = null!;

    [Required]
    public string Country { get; set; } = null!;

    [Required]
    public FestivalStatus Status { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public IFormFile? PictureFile { get; set; }
    
    public string? ExistingPictureUrl { get; set; }
}
