using LocalEcho.Core.Entities;

namespace LocalEcho.Core.Models;

public record MarkerWorkFilter(
    Guid? DistrictId,
    MarkerStatus? Status,
    MarkerCategory? Category,
    int Limit = 50
);