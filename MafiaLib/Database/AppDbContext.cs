using MafiaLib.Models;
using Microsoft.EntityFrameworkCore;

namespace MafiaLib.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
    {
        Database.EnsureCreated();
    }

    public DbSet<TgUser> Users { get; init; }
    public DbSet<GameResult> GameResults { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TgUser>()
            .HasMany(s => s.WinGames)
            .WithMany(c => c.Winners);
    }
}