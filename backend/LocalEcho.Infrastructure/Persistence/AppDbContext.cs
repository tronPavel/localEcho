using LocalEcho.Core.Entities;
using LocalEcho.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LocalEcho.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<Marker> Markers => Set<Marker>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Vote> Votes => Set<Vote>();
    
    public DbSet<MarkerImage> MarkerImages => Set<MarkerImage>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Включаем поддержку гео-индексов PostGIS
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<Marker>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).HasMaxLength(500);
            entity.Property(m => m.Category).HasConversion<string>().IsRequired();
            entity.Property(m => m.Status).HasConversion<string>().IsRequired();
            
            // Настройка нативной географической точки WGS84
            entity.Property(m => m.Location)
                  .HasColumnType("geometry(Point, 4326)")
                  .IsRequired();
                  
            entity.Property(m => m.CreatedAt).IsRequired();
            entity.Property(m => m.Rating).IsRequired();
            
            entity.HasIndex(m => m.Location).HasMethod("GIST");
            
            entity.HasOne(m => m.Creator)
                .WithMany()
                .HasForeignKey(m => m.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    
        modelBuilder.Entity<MarkerImage>(entity => {
            entity.HasKey(ei => ei.Id);
            entity.HasOne<Marker>()
                .WithMany(m => m.Images)
                .HasForeignKey(ei => ei.MarkerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Description).HasMaxLength(500);
            entity.Property(d => d.IconColor).HasMaxLength(7);
            
            entity.Property(d => d.Boundaries)
                  .HasColumnType("geometry(Polygon, 4326)")
                  .IsRequired();
                  
            entity.HasIndex(d => d.Boundaries).HasMethod("GIST");
            entity.Property(d => d.IsActive).IsRequired();
        });
        
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(v => new { v.MarkerId, v.UserId });
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Marker>().WithMany().HasForeignKey(v => v.MarkerId).OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.HomeLocation).HasColumnType("geometry(Point, 4326)");
            entity.HasIndex(u => u.HomeLocation).HasMethod("GIST");
            entity.HasIndex(u => u.DistrictId);
        });
    }
}