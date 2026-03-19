using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace LocalEcho.Infrastructure.Repositories;

public class IdentityRepository : IIdentityRepository, ITokenRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }


    public async Task<bool> CreateUserAsync(ApplicationUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        
        await _userManager.AddToRoleAsync(user, "User");
        return true;
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password) 
        => await _userManager.CheckPasswordAsync(user, password);

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user) 
        => await _userManager.GetRolesAsync(user);


    public async Task SetRefreshTokenAsync(ApplicationUser user, string refreshToken) 
        => await _userManager.SetAuthenticationTokenAsync(user, "LocalEcho", "RefreshToken", refreshToken);

    public async Task<string?> GetRefreshTokenAsync(ApplicationUser user) 
        => await _userManager.GetAuthenticationTokenAsync(user, "LocalEcho", "RefreshToken");

    public async Task RemoveRefreshTokenAsync(ApplicationUser user) 
        => await _userManager.RemoveAuthenticationTokenAsync(user, "LocalEcho", "RefreshToken");
}