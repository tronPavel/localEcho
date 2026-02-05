using System.ComponentModel.DataAnnotations;

namespace LocalEcho.Application.Dtos;

public record RegisterDto(
    [Required] [EmailAddress] string Email,
    [Required] [StringLength(100, MinimumLength = 6)] string Password,
    [Required] [property: Compare("Password")] string ConfirmPassword,
    [Required] string Name,
    [Required] Guid DistrictId = default,
    string? HomeAddress = null,
    bool RequestVerification = false,
    string? VerificationNote = null
);