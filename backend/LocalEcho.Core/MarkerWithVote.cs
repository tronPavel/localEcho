using LocalEcho.Core.Entities;

namespace LocalEcho.Core.Models;

public record MarkerWithVote(
    Marker Marker,   
    int UserVote   
);