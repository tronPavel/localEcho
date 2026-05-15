using Microsoft.AspNetCore.Http;

namespace LocalEcho.Application.Dtos;

public record UpdateProfileDto(
    string? Name,
    string? Bio, 
    string? HomeAddress,
    Guid? CityId,     
    Guid? DistrictId,
    IFormFile? AvatarFile 
);

public record UserProfileDto(
    Guid Id,
    string Email,
    string Name,
    string? Bio,     
    string? AvatarUrl,
    string? HomeAddress,
    int Points,
    DateTime CreatedAt,
    
    CityBriefDto? City,        
    DistrictBriefDto? District,
    IList<string> Roles,
    double? Latitude, 
    double? Longitude  
);