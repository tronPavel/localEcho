using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Data;

public class DataMarker
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public Point Location { get; set; } 
    public string? Description { get; set; }
    public string Category { get; set; } 
    public string Status { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}