namespace LocalEcho.Application.Dtos;

public record LeaderboardEntryDto(
    Guid Id,
    string Name,
    string? AvatarUrl, 
    int Points
);