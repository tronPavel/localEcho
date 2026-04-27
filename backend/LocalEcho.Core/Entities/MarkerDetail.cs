using LocalEcho.Core.Entities;
using LocalEcho.Core.Entities.Identity;

namespace LocalEcho.Core.Entities;

public record MarkerDetail(
    Marker Marker,
    ApplicationUser? Creator,
    int UserVote
);