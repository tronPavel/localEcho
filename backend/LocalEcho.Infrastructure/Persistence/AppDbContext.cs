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
    public DbSet<MarkerResolution> MarkerResolutions => Set<MarkerResolution>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<City> Cities => Set<City>();
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<Marker>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Description).HasMaxLength(2000);
            
            entity.Property(m => m.Category).HasConversion<string>().IsRequired();
            entity.Property(m => m.Status).HasConversion<string>().IsRequired();
            
            entity.Property(m => m.Location)
                .HasColumnType("geometry(Geometry, 4326)") 
                .IsRequired();
                  
            entity.Property(m => m.CreatedAt).IsRequired();
            entity.Property(m => m.Rating).IsRequired().HasDefaultValue(0);
            entity.Property(m => m.IsHidden).IsRequired().HasDefaultValue(false);
            
            entity.HasIndex(m => m.Location).HasMethod("GIST");
            
            entity.HasOne(m => m.Creator)
                .WithMany()
                .HasForeignKey(m => m.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict); 
            entity.Property(m => m.IsOfficial).HasDefaultValue(false);
            entity.HasIndex(m => m.CityId);
        });

        modelBuilder.Entity<MarkerImage>(entity => {
            entity.HasKey(ei => ei.Id);
            entity.Property(ei => ei.Url).IsRequired();

            entity.HasOne<Marker>()
                .WithMany(m => m.Images)
                .HasForeignKey(ei => ei.MarkerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<MarkerResolution>()
                .WithMany(r => r.Images)
                .HasForeignKey(ei => ei.MarkerResolutionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<MarkerResolution>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Comment).IsRequired().HasMaxLength(2000);

            entity.HasOne(r => r.Marker)
                .WithMany(m => m.Resolutions) 
                .HasForeignKey(r => r.MarkerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.ResolvedByUser)
                .WithMany() 
                .HasForeignKey(r => r.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict); 
        });
        
        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Boundaries).HasColumnType("geometry(Polygon, 4326)").IsRequired();
            entity.HasIndex(c => c.Boundaries).HasMethod("GIST");
        });
        
        modelBuilder.Entity<District>(entity =>
        {
            entity.HasOne(d => d.City)
                .WithMany()
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            
            entity.Property(d => d.Boundaries)
                  .HasColumnType("geometry(Polygon, 4326)")
                  .IsRequired();
                  
            entity.HasIndex(d => d.Boundaries).HasMethod("GIST");
            entity.Property(d => d.IsActive).IsRequired().HasDefaultValue(true);
        });
        
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(v => new { v.MarkerId, v.UserId });

            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Marker>().WithMany().HasForeignKey(v => v.MarkerId).OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Report>(entity => {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Reason).HasConversion<string>();

            entity.HasOne<Marker>()
                .WithMany()
                .HasForeignKey(r => r.MarkerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.SetNull); 
        });
        
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.HomeLocation).HasColumnType("geometry(Point, 4326)");
            entity.Property(u => u.Bio).HasMaxLength(1000);
            entity.HasIndex(u => u.HomeLocation).HasMethod("GIST");
            entity.HasIndex(u => u.DistrictId);
        });
    }
}