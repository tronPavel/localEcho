namespace LocalEcho.Application.Dtos;

public record DistrictDto(
    Guid Id,
    string Name,
    string? Description,
    double CenterLat,
    double CenterLng,
    string? IconColor
);

