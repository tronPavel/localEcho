using LocalEcho.Application.Dtos;

namespace LocalEcho.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task<bool> LogoutAsync(Guid userId);
    Task<object> GetProfileAsync(Guid userId);
    Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<bool> ChangeDistrictAsync(Guid userId, ChangeDistrictDto dto);
    Task<IEnumerable<object>> GetAllDistrictsAsync();
    Task<bool> UpdateAvatarAsync(Guid userId, string avatarUrl);
}