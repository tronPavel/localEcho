namespace LocalEcho.Application.Dtos;

public record UserProfileDto(
    Guid Id,
    string Email,
    string Name,
    string? AvatarUrl,
    string? HomeAddress,
    bool IsVerified,
    int Points,
    DateTime CreatedAt,
    DistrictDto? District,
    IList<string> Roles,
    double? Latitude, 
    double? Longitude  
);