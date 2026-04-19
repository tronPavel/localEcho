using Microsoft.AspNetCore.Http;

namespace LocalEcho.Application.Dtos;

public record UpdateProfileDto(
    string? Name,
    string? HomeAddress,
    Guid? DistrictId,
    IFormFile? AvatarFile 
);

public record UserProfileDto(
    Guid Id,
    string Email,
    string Name,
    string? AvatarUrl,
    string? HomeAddress,
    int Points,
    DateTime CreatedAt,
    DistrictBriefDto? District,
    IList<string> Roles,
    double? Latitude, 
    double? Longitude  
);