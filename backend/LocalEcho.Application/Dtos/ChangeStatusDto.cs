using LocalEcho.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace LocalEcho.Application.Dtos;

public record ChangeStatusDto(
    MarkerStatus NewStatus,
    string? Comment,         
    List<IFormFile>? ImageFiles
);