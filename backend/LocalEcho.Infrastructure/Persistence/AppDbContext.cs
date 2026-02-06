using LocalEcho.Core.Entities;
using LocalEcho.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<Marker> Markers => Set<Marker>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Vote> Votes => Set<Vote>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");

        // Конвертер для GeoPoint
        var geoConverter = new ValueConverter<GeoPoint, Point>(
            geo => new Point(geo.Longitude, geo.Latitude) { SRID = 4326 },
            point => new GeoPoint(point.Y, point.X)
        );

        modelBuilder.Entity<Marker>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).HasMaxLength(500);
            entity.Property(m => m.Category).HasConversion<string>().IsRequired();
            entity.Property(m => m.Status).HasConversion<string>().IsRequired();
            entity.Property(m => m.Location).HasConversion(geoConverter).HasColumnType("geometry(Point, 4326)").IsRequired();
            entity.Property(m => m.CreatedAt).IsRequired();
            entity.Property(m => m.UpdatedAt);
            entity.Property(m => m.Rating).IsRequired();
            entity.Property(m => m.ImageUrl).HasMaxLength(2048);
            entity.HasIndex(m => m.Location).HasMethod("GIST");
        });
        
        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Description).HasMaxLength(500);
            entity.Property(d => d.CenterLat).IsRequired();
            entity.Property(d => d.CenterLng).IsRequired();
            entity.Property(d => d.IconColor).HasMaxLength(7);
            entity.Property(d => d.IsActive).IsRequired();
            entity.Property(d => d.CreatedAt).IsRequired();
        });
        
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(v => new { v.MarkerId, v.UserId }); // Составной ключ
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Marker>().WithMany().HasForeignKey(v => v.MarkerId).OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.DistrictId);
        });
    }
}