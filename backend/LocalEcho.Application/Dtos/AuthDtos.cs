using System.ComponentModel.DataAnnotations;


namespace LocalEcho.Application.Dtos;

public record AuthResponseDto(
    string Token,
    string RefreshToken,
    DateTime Expires,
    string UserId,
    string Email,
    string Name,
    string? AvatarUrl,
    Guid? DistrictId,
    string? DistrictName,
    int Points,
    List<string> Roles,
    double? Latitude,  
    double? Longitude  
);

public record RefreshTokenDto(
    [Required] string Token,
    [Required] string RefreshToken
);

public record LoginDto(
    [Required] [EmailAddress] string Email,
    [Required] string Password
);

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
    
    public Guid? DistrictId { get; init; }

    public string? HomeAddress { get; init; }
}