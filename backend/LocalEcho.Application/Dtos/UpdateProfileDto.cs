using Microsoft.AspNetCore.Http;

namespace LocalEcho.Application.Dtos;

public record UpdateProfileDto(
    string? Name,
    string? HomeAddress,
    Guid? DistrictId,
    IFormFile? AvatarFile 
);