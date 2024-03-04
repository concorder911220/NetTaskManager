using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
            user.HasKey(u => u.Sub);
        });
    }
}