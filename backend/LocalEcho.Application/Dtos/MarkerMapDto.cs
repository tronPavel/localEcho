using LocalEcho.Core.Entities;

namespace LocalEcho.Application.Dtos;

public record MarkerMapDto(
    Guid Id,
    string Title,
    MarkerCategory Category,
    MarkerStatus Status,
    string GeometryType,           
    List<CoordinateDto> Coordinates, 
    CoordinateDto Centroid   
);