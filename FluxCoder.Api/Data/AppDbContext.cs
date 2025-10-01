using FluxCoder.Api.Models;
using Microsoft.EntityFrameworkCore;
using Stream = FluxCoder.Api.Models.Stream;


namespace FluxCoder.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{   
    public DbSet<User> Users { get; set; }
    public DbSet<Stream> Streams { get; set; }
    public DbSet<TranscodingJob> TranscodingJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
        });
        
        modelBuilder.Entity<Stream>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<TranscodingJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            
            entity.HasOne(e => e.Stream)
                .WithMany()
                .HasForeignKey(e => e.StreamId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = adminPasswordHash,
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );
    }
}