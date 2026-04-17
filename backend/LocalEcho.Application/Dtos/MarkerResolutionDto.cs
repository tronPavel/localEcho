namespace LocalEcho.Application.Dtos;

public record MarkerResolutionDto(
    string Comment,
    string AuthorName,
    DateTime CreatedAt,
    List<string> ImageUrls
);