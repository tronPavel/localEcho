using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LocalEcho.Application.Interfaces;
using LocalEcho.Application.Dtos;
using LocalEcho.Core.Interfaces;
using LocalEcho.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LocalEcho.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDistrictRepository _districtRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IDistrictRepository districtRepository,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _districtRepository = districtRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null) throw new Exception("Email already exists");

        var district = await _districtRepository.GetByIdAsync(dto.DistrictId) 
                       ?? throw new Exception("District not found");

        var user = new ApplicationUser
        {
            UserName = dto.Email, // Используем Email как логин
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

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) 
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "User");

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new Exception("Invalid credentials");

        user.LastSeen = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        
        await _userManager.ResetAccessFailedCountAsync(user);

        return await GenerateTokensAsync(user);
    }

    private async Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.Name ?? "User"), 
            new("DistrictId", user.DistrictId?.ToString() ?? ""),
            new("IsVerified", user.IsVerified.ToString())
        };
        
        foreach (var role in userRoles) 
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = GenerateJwtToken(claims);
        var refreshToken = GenerateRefreshToken();

        await _userManager.SetAuthenticationTokenAsync(user, "LocalEcho", "RefreshToken", refreshToken);

        var districtName = await _districtRepository.GetNameByIdAsync(user.DistrictId ?? Guid.Empty);

        return new AuthResponseDto(
            token,
            refreshToken,
            DateTime.UtcNow.AddMinutes(GetTokenLifetime()),
            user.Id.ToString(),
            user.Email!,
            user.Name ?? "User",
            user.AvatarUrl,
            user.DistrictId,
            districtName,
            user.IsVerified,
            user.Points,
            userRoles.ToList()
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

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var principal = GetPrincipalFromExpiredToken(dto.Token);
        var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? throw new Exception("Invalid token claims");
        
        var userId = Guid.Parse(userIdStr);
        var user = await _userManager.FindByIdAsync(userId.ToString()) 
                   ?? throw new Exception("User not found");

        var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "LocalEcho", "RefreshToken");
        
        if (storedToken != dto.RefreshToken) 
            throw new Exception("Invalid refresh token");

        await _userManager.RemoveAuthenticationTokenAsync(user, "LocalEcho", "RefreshToken");

        return await GenerateTokensAsync(user);
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

        if (securityToken is not JwtSecurityToken jwt || 
            !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token signature");
        }

        return principal;
    }

    public async Task<bool> LogoutAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        await _userManager.RemoveAuthenticationTokenAsync(user, "LocalEcho", "RefreshToken");
        return true;
    }

    public async Task<object> GetProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString()) 
                   ?? throw new Exception("User not found");
        
        var roles = await _userManager.GetRolesAsync(user);
        var district = await _districtRepository.GetByIdAsync(user.DistrictId ?? Guid.Empty);

        return new
        {
            user.Id,
            user.Email,
            user.Name, 
            user.AvatarUrl,
            user.HomeAddress,
            user.IsVerified,
            user.Points,
            user.LastSeen,
            user.CreatedAt,
            District = district != null 
                ? new { district.Id, district.Name, district.Description, district.IconColor } 
                : null,
            Roles = roles
        };
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        if (dto.Name != null) user.Name = dto.Name; 
        if (dto.HomeAddress != null) user.HomeAddress = dto.HomeAddress;
        
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ChangeDistrictAsync(Guid userId, ChangeDistrictDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var district = await _districtRepository.GetByIdAsync(dto.DistrictId) 
                       ?? throw new Exception("District not found");

        user.DistrictId = dto.DistrictId;
        user.HomeLatitude = district.CenterLat;
        user.HomeLongitude = district.CenterLng;
        
        if (dto.HomeAddress != null) user.HomeAddress = dto.HomeAddress;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<IEnumerable<object>> GetAllDistrictsAsync()
    {
        var districts = await _districtRepository.GetAllActiveAsync();
        return districts.Select(d => new { d.Id, d.Name, d.Description, d.CenterLat, d.CenterLng, d.IconColor });
    }

    private int GetTokenLifetime() => _configuration.GetValue<int>("JwtSettings:TokenLifetimeMinutes", 60);
    
    public async Task<bool> UpdateAvatarAsync(Guid userId, string avatarUrl)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.AvatarUrl = avatarUrl;
    
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}