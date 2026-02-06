using System.ComponentModel.DataAnnotations;

namespace LocalEcho.Application.Dtos;

public record RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; init; } = string.Empty;

    [Required]
    public string Name { get; init; } = string.Empty; 

    [Required]
    public Guid DistrictId { get; init; }

    public string? HomeAddress { get; init; }
}