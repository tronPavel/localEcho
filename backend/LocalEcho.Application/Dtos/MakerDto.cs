using LocalEcho.Core.Entities;

public record MarkerDto(
    Guid Id,
    string Title,
    double Latitude,
    double Longitude,
    string? Description,
    string? ImageUrl,
    MarkerCategory Category,
    MarkerStatus Status,
    Guid CreatorId,
    int Rating,      
    int UserVote, 
    DateTime CreatedAt,
    DateTime? UpdatedAt);