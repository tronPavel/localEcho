using LocalEcho.Core.Entities.Identity;

namespace LocalEcho.Application.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    
    Task<bool> CreateAsync(ApplicationUser user, string password);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task UpdateAsync(ApplicationUser user);
    
    Task<IEnumerable<ApplicationUser>> GetTopUsersAsync(int count, Guid? districtId);
    
    Task SetRefreshTokenAsync(ApplicationUser user, string refreshToken);
    Task<string?> GetRefreshTokenAsync(ApplicationUser user);
    Task RemoveRefreshTokenAsync(ApplicationUser user);
}