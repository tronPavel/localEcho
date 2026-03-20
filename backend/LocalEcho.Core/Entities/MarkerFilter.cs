using LocalEcho.Core.Entities;

namespace LocalEcho.Core.Models;

public record MarkerFilter
{
    public MarkerCategory? Category { get; init; }
    public MarkerStatus? Status { get; init; }
    
    public double? MinLat { get; init; }
    public double? MaxLat { get; init; }
    public double? MinLng { get; init; }
    public double? MaxLng { get; init; }
    
    public int? Limit { get; init; }

    public bool HasBoundingBox() => 
        MinLat.HasValue && MaxLat.HasValue && 
        MinLng.HasValue && MaxLng.HasValue;
}