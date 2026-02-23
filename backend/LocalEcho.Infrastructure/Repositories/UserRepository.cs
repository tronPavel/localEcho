using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocalEcho.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id) 
        => await _userManager.FindByIdAsync(id.ToString());

    public async Task<ApplicationUser?> GetByEmailAsync(string email) 
        => await _userManager.FindByEmailAsync(email);

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user) 
        => await _userManager.GetRolesAsync(user);

    public async Task<bool> CreateAsync(ApplicationUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
             throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        
        await _userManager.AddToRoleAsync(user, "User");
        return true;
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password) 
        => await _userManager.CheckPasswordAsync(user, password);

    public async Task UpdateAsync(ApplicationUser user)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<IEnumerable<ApplicationUser>> GetTopUsersAsync(int count, Guid? districtId)
    {
        var query = _userManager.Users.AsNoTracking();

        if (districtId.HasValue)
            query = query.Where(u => u.DistrictId == districtId.Value);

        return await query
            .OrderByDescending(u => u.Points)
            .Take(count)
            .ToListAsync();
    }

    public async Task SetRefreshTokenAsync(ApplicationUser user, string refreshToken) 
        => await _userManager.SetAuthenticationTokenAsync(user, "LocalEcho", "RefreshToken", refreshToken);

    public async Task<string?> GetRefreshTokenAsync(ApplicationUser user) 
        => await _userManager.GetAuthenticationTokenAsync(user, "LocalEcho", "RefreshToken");

    public async Task RemoveRefreshTokenAsync(ApplicationUser user) 
        => await _userManager.RemoveAuthenticationTokenAsync(user, "LocalEcho", "RefreshToken");
}