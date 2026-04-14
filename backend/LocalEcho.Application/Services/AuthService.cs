using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities.Identity;
using LocalEcho.Core.Exceptions;
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
        if (existingUser != null) 
            throw new BadRequestException("Пользователь с таким Email уже зарегистрирован.");

        if (dto.DistrictId.HasValue)
        {
            var districtExists = await _districtRepository.GetByIdAsync(dto.DistrictId.Value);
            if (districtExists == null) throw new KeyNotFoundException("Указанный район не существует.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Name = dto.Name,
            DistrictId = dto.DistrictId,
            HomeAddress = dto.HomeAddress,
            HomeLocation = null,
            CreatedAt = DateTime.UtcNow,
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
            throw new BadRequestException("Неверный Email или пароль.");
        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var principal = GetPrincipalFromExpiredToken(dto.Token);
        var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? throw new SecurityTokenException("Недействительный токен доступа.");
        
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userIdStr)) 
                   ?? throw new SecurityTokenException("Пользователь не найден.");

        var storedToken = await _tokenRepository.GetRefreshTokenAsync(user);
        
        if (storedToken != dto.RefreshToken) 
            throw new SecurityTokenException("Сессия устарела или недействительна. Пожалуйста, войдите заново.");

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

        double? lat = user.HomeLocation?.Y;
        double? lng = user.HomeLocation?.X;

        var districtName = user.DistrictId.HasValue 
            ? await _districtRepository.GetNameByIdAsync(user.DistrictId.Value) 
            : null;

        return new AuthResponseDto(
            token, refreshToken, DateTime.UtcNow.AddMinutes(GetTokenLifetime()),
            user.Id.ToString(), user.Email!, user.Name ?? "User", user.AvatarUrl,
            user.DistrictId, districtName, user.IsVerified, user.Points, userRoles.ToList(),
            lat, lng 
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