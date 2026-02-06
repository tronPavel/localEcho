namespace LocalEcho.Application.Dtos;

public record UpdateProfileDto(
    string? Name,
    string? AvatarUrl,
    string? HomeAddress
);
