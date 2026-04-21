using System.ComponentModel.DataAnnotations;
using BoomFest.Enums;

namespace BoomFest.Dtos;

public class FestivalDetailsDto
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Location is required")]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    [Required]
    public FestivalStatus Status { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public IFormFile? PictureFile { get; set; }

    public string? ExistingPictureUrl { get; set; }

    public List<FestivalLineupEditDto> Lineups { get; set; } = new();

    public List<FestivalTicketTierEditDto> TicketCategories { get; set; } = new();

    public decimal LiveRevenueRon { get; set; }

    public int SoldTickets { get; set; }

    public int TotalTicketCapacity { get; set; }

    public int SoldPercentage => TotalTicketCapacity <= 0
        ? 0
        : (int)Math.Round((double)SoldTickets / TotalTicketCapacity * 100);
}

public class FestivalLineupEditDto
{
    public Guid? Id { get; set; }

    [StringLength(120)]
    public string ArtistName { get; set; } = string.Empty;

    [StringLength(120)]
    public string? SlotDetails { get; set; }

    public string? PictureUrl { get; set; }

    public bool IsDeleted { get; set; }
}

public class FestivalTicketTierEditDto
{
    public Guid? Id { get; set; }

    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Description { get; set; }

    [Range(0, 100000)]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public int Stock { get; set; }

    public bool IsDeleted { get; set; }
}

