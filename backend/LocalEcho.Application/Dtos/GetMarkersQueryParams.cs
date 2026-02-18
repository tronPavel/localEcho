namespace LocalEcho.Application.Dtos;

public record GetMarkersQueryParams
{
    public string? Category { get; init; }
    public string? Status { get; init; }
    
    public double? MinLat { get; init; }
    public double? MaxLat { get; init; }
    public double? MinLng { get; init; }
    public double? MaxLng { get; init; }
}