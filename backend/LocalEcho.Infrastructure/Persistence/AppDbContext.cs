using LocalEcho.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Marker> Markers => Set<Marker>(); // Да, можно сохранять доменный Marker!

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");

        // Конвертер: GeoPoint ↔ NetTopologySuite.Point
        var geoConverter = new ValueConverter<GeoPoint, Point>(
            geo => new Point(geo.Longitude, geo.Latitude) { SRID = 4326 },   // C# → БД
            point => new GeoPoint(point.Y, point.X)                          // БД → C#
        );

        modelBuilder.Entity<Marker>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id)
                  .ValueGeneratedNever(); // Мы сами задаём Guid в конструкторе

            entity.Property(m => m.Title)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(m => m.Description)
                  .HasMaxLength(500);

            entity.Property(m => m.Category)
                  .HasConversion<string>()    // enum → "Issue", "Event"...
                  .IsRequired();

            entity.Property(m => m.Status)
                  .HasConversion<string>()
                  .IsRequired();

            // Вот здесь магия — говорим EF, как работать с GeoPoint
            entity.Property(m => m.Location)
                  .HasConversion(geoConverter)           // ← КЛЮЧЕВАЯ СТРОКА
                  .HasColumnType("geometry(Point, 4326)")
                  .IsRequired();

            entity.Property(m => m.CreatedAt).IsRequired();
            entity.Property(m => m.UpdatedAt);

            // GIST-индекс для быстрых гео-запросов (в радиусе и т.д.)
            entity.HasIndex(m => m.Location)
                  .HasMethod("GIST");
        });
    }
}