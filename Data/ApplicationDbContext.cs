using BoomFest.Models;
using Microsoft.EntityFrameworkCore;

namespace BoomFest.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Festival> Festivals => Set<Festival>();
    public DbSet<Lineup> Lineups => Set<Lineup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<TicketCategory>()
            .Property(ticketCategory => ticketCategory.Price)
            .HasPrecision(10, 2);
        
        modelBuilder.Entity<Order>()
            .Property(order => order.TotalPrice)
            .HasPrecision(10, 2);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var entries = ChangeTracker.Entries<Model>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}
