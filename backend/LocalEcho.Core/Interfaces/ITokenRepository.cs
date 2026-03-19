using LocalEcho.Core.Entities.Identity;

namespace LocalEcho.Application.Interfaces;

public interface ITokenRepository
{
    Task SetRefreshTokenAsync(ApplicationUser user, string refreshToken);
    Task<string?> GetRefreshTokenAsync(ApplicationUser user);
    Task RemoveRefreshTokenAsync(ApplicationUser user);
}