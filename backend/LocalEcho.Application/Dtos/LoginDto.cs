using System.ComponentModel.DataAnnotations;

namespace LocalEcho.Application.Dtos;

public record LoginDto(
    [Required] [EmailAddress] string Email,
    [Required] string Password
);