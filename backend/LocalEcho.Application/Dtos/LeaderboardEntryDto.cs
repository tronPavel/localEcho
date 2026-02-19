namespace LocalEcho.Application.Dtos;

public record LeaderboardEntryDto(
    Guid Id,
    string Name,
    int Points
);