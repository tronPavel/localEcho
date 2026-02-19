using LocalEcho.Core.Entities;

namespace LocalEcho.Core.Models;

public record MarkerPreview(
    Guid Id,
    string Title,
    GeoPoint Location,
    MarkerCategory Category,
    MarkerStatus Status
);