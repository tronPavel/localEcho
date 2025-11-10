namespace LocalEcho.Application.Dtos;

using LocalEcho.Core.Entities;

public record MarkerDto(
    Guid Id,
    string Title,
    double Latitude,
    double Longitude,
    string? Description,
    MarkerCategory Category,
    MarkerStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);