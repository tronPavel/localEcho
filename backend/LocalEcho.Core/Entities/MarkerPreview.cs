using LocalEcho.Core.Entities;

namespace LocalEcho.Core.Models;

public record MarkerPreview(
    Guid Id,
    string Title,
    double Latitude, 
    double Longitude,
    MarkerCategory Category,
    MarkerStatus Status
);