namespace LocalEcho.Core.Models.Marker;

public record MarkerWorkTask(
    Guid Id,
    string Title,
    string Category,
    string Status,
    string CreatorName,
    string DistrictName,
    DateTime CreatedAt,
    int Rating
);