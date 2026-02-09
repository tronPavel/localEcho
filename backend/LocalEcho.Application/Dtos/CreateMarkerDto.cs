using LocalEcho.Core.Entities;

public record CreateMarkerDto(
    string Title, 
    double Latitude,
    double Longitude, 
    MarkerCategory Category, 
    string? Description = null,
    string? ImageUrl = null);