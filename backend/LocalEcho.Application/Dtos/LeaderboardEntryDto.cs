namespace LocalEcho.Application.Dtos;

public record LeaderboardEntryDto(
    Guid UserId,
    string UserName,
    int Points
);