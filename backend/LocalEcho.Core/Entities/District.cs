using NetTopologySuite.Geometries;

namespace LocalEcho.Core.Entities;

public class District
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconColor { get; private set; } = "#3b82f6";
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Polygon Boundaries { get; private set; } = null!;

    public Point Centroid => Boundaries.Centroid; 

    private District() { } 

    public static District Create(string name, Polygon boundaries, string? description = null, string? iconColor = "#3b82f6")
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required", nameof(name));
        if (boundaries == null || !boundaries.IsValid) throw new ArgumentException("Invalid boundary", nameof(boundaries));

        return new District
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Boundaries = boundaries,
            IconColor = iconColor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}