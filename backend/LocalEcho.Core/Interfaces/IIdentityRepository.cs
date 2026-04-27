using LocalEcho.Core.Entities.Identity;

namespace LocalEcho.Application.Interfaces;

public interface IIdentityRepository
{
    Task<bool> CreateUserAsync(ApplicationUser user, string password);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    
    Task<bool> AddToRoleAsync(ApplicationUser user, string roleName);
    
    Task<bool> RemoveFromRoleAsync(ApplicationUser user, string roleName);
}