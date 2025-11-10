using LocalEcho.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;

namespace LocalEcho.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<DataMarker> Markers { get; set; } = null!; // null! для nullable reference types.

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis"); // Включаем PostGIS.

        modelBuilder.Entity<DataMarker>(entity =>
        {
            entity.HasKey(e => e.Id); // Primary Key.
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200); // Обязательное, до 200 символов.
            entity.Property(e => e.Location).HasColumnType("geometry (point, 4326)").IsRequired(); // Точка, SRID=4326, обязательное.
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50); // String для читаемости.
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500); // До 500 символов.
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt); // Nullable.

            // Индекс для гео-запросов (для будущего поиска по радиусу).
            entity.HasIndex(e => e.Location).HasMethod("GIST"); // GIST индекс для PostGIS.
        });

        base.OnModelCreating(modelBuilder); // Безопасность для EF расширений.
    }
}