using LocalEcho.Core.Entities;

namespace LocalEcho.Core.Models;

public record MarkerWithVote(
    Marker Marker,   // Сама сущность маркера
    int UserVote     // -1, 0 или 1 (голос текущего пользователя)
);