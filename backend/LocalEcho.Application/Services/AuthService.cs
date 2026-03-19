using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities.Identity;
using LocalEcho.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LocalEcho.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityRepository _identityRepository; 
    private readonly ITokenRepository _tokenRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IIdentityRepository identityRepository, 
        ITokenRepository tokenRepository,
        IDistrictRepository districtRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _identityRepository = identityRepository;
        _tokenRepository = tokenRepository;
        _districtRepository = districtRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null) throw new Exception("Email already exists");

        var district = await _districtRepository.GetByIdAsync(dto.DistrictId) 
                       ?? throw new Exception("District not found");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Name = dto.Name,
            DistrictId = dto.DistrictId,
            HomeAddress = dto.HomeAddress,
            HomeLatitude = district.CenterLat, 
            HomeLongitude = district.CenterLng,
            CreatedAt = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow,
            IsVerified = false,
            Points = 0
        };

        await _identityRepository.CreateUserAsync(user, dto.Password);
        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || !await _identityRepository.CheckPasswordAsync(user, dto.Password))
            throw new Exception("Invalid credentials");

        user.LastSeen = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var principal = GetPrincipalFromExpiredToken(dto.Token);
        var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? throw new Exception("Invalid token claims");
        
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userIdStr)) 
                   ?? throw new Exception("User not found");

        var storedToken = await _tokenRepository.GetRefreshTokenAsync(user);
        
        if (storedToken != dto.RefreshToken) 
            throw new Exception("Invalid refresh token");

        await _tokenRepository.RemoveRefreshTokenAsync(user);

        return await GenerateTokensAsync(user);
    }

    public async Task LogoutAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            await _tokenRepository.RemoveRefreshTokenAsync(user);
        }
    }
    
    private async Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user)
    {
        var userRoles = await _identityRepository.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.Name ?? "User"), 
            new("DistrictId", user.DistrictId?.ToString() ?? ""),
            new("IsVerified", user.IsVerified.ToString())
        };
        
        foreach (var role in userRoles) claims.Add(new Claim(ClaimTypes.Role, role));

        var token = GenerateJwtToken(claims);
        var refreshToken = GenerateRefreshToken();

        await _tokenRepository.SetRefreshTokenAsync(user, refreshToken);

        var districtName = await _districtRepository.GetNameByIdAsync(user.DistrictId ?? Guid.Empty);

        return new AuthResponseDto(
            token, refreshToken, DateTime.UtcNow.AddMinutes(GetTokenLifetime()),
            user.Id.ToString(), user.Email!, user.Name ?? "User", user.AvatarUrl,
            user.DistrictId, districtName, user.IsVerified, user.Points, userRoles.ToList()
        );
    }

    private string GenerateJwtToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetTokenLifetime()),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateAudience = false, 
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!)),
            ValidateLifetime = false 
        };
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, parameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwt || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token signature");
        return principal;
    }

    private int GetTokenLifetime() => _configuration.GetValue<int>("JwtSettings:TokenLifetimeMinutes", 60);
}