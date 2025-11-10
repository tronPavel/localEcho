using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Data;

public class DataMarker
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public Point Location { get; set; } // NTS Point для PostGIS: X=Longitude, Y=Latitude.
    public string? Description { get; set; }
    public string Category { get; set; } // Enum как string: потому что PostGIS/PostgreSQL хранит enums как строки для простоты.
    public string Status { get; set; } // Аналогично.
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}