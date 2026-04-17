using LocalEcho.Core.Entities;

namespace LocalEcho.Application.Dtos;

public record MarkerDetailDto(
    Guid Id,
    string Title,
    string? Description,
    List<string> ImageUrls, 
    MarkerCategory Category,
    MarkerStatus Status,
    Guid CreatorId,
    string CreatorName,
    string? CreatorAvatarUrl,
    int Rating,
    int UserVote,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    MarkerResolutionDto? Resolution 
);