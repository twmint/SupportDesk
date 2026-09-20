using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.Reference)
            .IsUnique();
    }

    public DbSet<Agent> Agents { get; set; } = null!;

    public DbSet<Ticket> Tickets { get; set; } = null!;

    public DbSet<Comment> Comments { get; set; } = null!;
}