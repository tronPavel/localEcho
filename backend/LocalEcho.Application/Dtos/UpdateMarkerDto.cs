using Microsoft.AspNetCore.Http;

namespace LocalEcho.Application.Dtos;

public record UpdateMarkerDto(
    string Title,
    string? Description,
    List<IFormFile>? NewImageFiles,  
    List<string>? KeepImageUrls      
);