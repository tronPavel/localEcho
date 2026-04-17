namespace LocalEcho.Application.Dtos;

public record AuthResponseDto(
    string Token,
    string RefreshToken,
    DateTime Expires,
    string UserId,
    string Email,
    string Name,
    string? AvatarUrl,
    Guid? DistrictId,
    string? DistrictName,
    int Points,
    List<string> Roles,
    double? Latitude,  
    double? Longitude  
);