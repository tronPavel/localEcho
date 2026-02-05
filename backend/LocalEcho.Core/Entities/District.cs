namespace LocalEcho.Core.Entities;

public class District
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public double CenterLat { get; private set; }
    public double CenterLng { get; private set; }
    public string? IconColor { get; private set; } = "#3b82f6";
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private District() { } 

    public static District Create(string name, double centerLat, double centerLng, string? description = null, string? iconColor = "#3b82f6")
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required", nameof(name));

        return new District
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            CenterLat = centerLat,
            CenterLng = centerLng,
            IconColor = iconColor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDescription(string? newDescription)
    {
        Description = newDescription?.Trim();
    }
}