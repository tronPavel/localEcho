using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities.Identity;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocalEcho.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id) 
        => await _userManager.FindByIdAsync(id.ToString());

    public async Task<ApplicationUser?> GetByEmailAsync(string email) 
        => await _userManager.FindByEmailAsync(email);

    public async Task UpdateAsync(ApplicationUser user)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task UpdatePointsAsync(Guid userId, int delta, CancellationToken ct = default)
    {
        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Points, u => u.Points + delta), ct);
    }

    public async Task<IEnumerable<ApplicationUser>> GetTopUsersAsync(int count, Guid? districtId)
    {
        var query = _context.Users.AsNoTracking();

        if (districtId.HasValue)
            query = query.Where(u => u.DistrictId == districtId.Value);

        return await query
            .OrderByDescending(u => u.Points)
            .Take(count)
            .ToListAsync();
    }
}