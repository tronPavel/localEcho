using NetTopologySuite.Geometries;

namespace LocalEcho.Core.Entities;

public class District
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    
    public Polygon Boundaries { get; private set; } = null!;
    public Point Centroid => Boundaries.Centroid;

    private District() { } 

    public static District Create(string name, Polygon boundaries, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Имя района обязательно");
        if (boundaries == null || !boundaries.IsValid) throw new ArgumentException("Неверная геометрия");

        return new District
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Boundaries = boundaries,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, bool isActive)
    {
        Name = name.Trim();
        Description = description?.Trim();
        IsActive = isActive;
    }

    public void UpdateGeometry(Polygon boundaries)
    {
        Boundaries = boundaries ?? throw new ArgumentException("Геометрия не может быть пустой");
    }
    public void SetActive(bool isActive) => IsActive = isActive;
}