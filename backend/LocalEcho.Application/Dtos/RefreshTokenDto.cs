using System.ComponentModel.DataAnnotations;

namespace LocalEcho.Application.Dtos;

public record RefreshTokenDto(
    [Required] string Token,
    [Required] string RefreshToken
);