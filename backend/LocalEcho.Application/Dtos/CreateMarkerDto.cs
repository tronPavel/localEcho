using Microsoft.AspNetCore.Http;
using LocalEcho.Core.Entities;

public record CoordinateDto(double Lat, double Lng);
public record CreateMarkerDto(
    string Title, 
    MarkerCategory Category, 
    string? Description,
    List<IFormFile>? ImageFiles,
    List<CoordinateDto>? Points,
    DateTime? ScheduledAt
);