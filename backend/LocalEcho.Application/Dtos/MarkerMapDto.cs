using LocalEcho.Core.Entities;

namespace LocalEcho.Application.Dtos;

public record MarkerMapDto(
    Guid Id,
    double Latitude,
    double Longitude,
    MarkerCategory Category,
    MarkerStatus Status,
    string Title
);