using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities;
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

    public async Task<IEnumerable<RankingRecord>> GetLeaderboardAsync(int count, Guid? districtId)
    {
        if (districtId.HasValue && districtId.Value != Guid.Empty)
        {
            return await _context.Markers
                .AsNoTracking()
                .Where(m => m.DistrictId == districtId.Value)
                .GroupBy(m => m.CreatedByUserId)
                .Select(g => new { UserId = g.Key, LocalPoints = g.Sum(m => m.Rating) })
                .OrderByDescending(x => x.LocalPoints)
                .Take(count)
                .Join(_context.Users, stat => stat.UserId, user => user.Id, (stat, user) => 
                    new RankingRecord(user.Id, user.Name ?? "Аноним", user.AvatarUrl, stat.LocalPoints))
                .ToListAsync();
        }

        return await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.Points)
            .Take(count)
            .Select(u => new RankingRecord(u.Id, u.Name ?? "Аноним", u.AvatarUrl, u.Points))
            .ToListAsync();
    }
}