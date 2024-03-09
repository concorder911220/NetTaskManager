using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<Assignment> Assignments { get; set; } = null!;
    public DbSet<Section> Sections { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
            user.Property(u => u.Sub).IsRequired();
        });

        modelBuilder.Entity<Board>(board =>
        {
            board.HasKey(b => b.Id);
            
            board.HasMany(b => b.Sections)
                .WithOne(s => s.Board)
                .HasForeignKey(s => s.BoardId)
                .IsRequired(false);

            board.HasOne(b => b.User)
                .WithMany(u => u.Boards)
                .HasForeignKey(b => b.UserId)
                .IsRequired();
        });
        
        modelBuilder.Entity<Section>(section =>
        {
            section.HasKey(s => s.Id);

            section.HasMany(s => s.Assignments)
                .WithOne(u => u.Section)
                .HasForeignKey(u => u.SectionId)
                .IsRequired();
        });
        
        modelBuilder.Entity<Assignment>(assignment =>
        {
            assignment.HasKey(a => a.Id);

            assignment.HasOne(a => a.CreatedBy)
                .WithMany(u => u.Assignments)
                .HasPrincipalKey(u => u.Id)
                .HasForeignKey(a => a.CreatedById);

            assignment.HasOne(a => a.Board)
                .WithMany(b => b.Assignments)
                .HasForeignKey(a => a.BoardId);

            assignment.HasMany(a => a.Tags)
                .WithMany(t => t.Assignments);
        });
        
        modelBuilder.Entity<Tag>(tag =>
        {
            tag.HasKey(t => t.Id);

            tag.HasOne(t => t.Board)
                .WithMany(b => b.Tags)
                .HasForeignKey(t => t.BoardId);
        });
    }
}