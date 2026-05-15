using NetTopologySuite.Geometries;

namespace LocalEcho.Core.Entities;

public class City
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Polygon Boundaries { get; private set; } = null!;

    private City() { }

    public static City Create(string name, Polygon boundaries)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required");
        return new City
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Boundaries = boundaries,
        };
    }
    
    public void Update(string name, Polygon boundaries)
    {
        Name = name;
        Boundaries = boundaries;
    }
}