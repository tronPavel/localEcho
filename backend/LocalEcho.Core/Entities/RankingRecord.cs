namespace LocalEcho.Core.Entities;

public record RankingRecord(
    Guid UserId, 
    string Name, 
    string? AvatarUrl, 
    int Points
);