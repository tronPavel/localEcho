using LocalEcho.Core.Entities;
using LocalEcho.Core.Entities.Identity;

namespace LocalEcho.Application.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task UpdateAsync(ApplicationUser user);
    Task UpdatePointsAsync(Guid userId, int delta, CancellationToken ct = default);
    Task<IEnumerable<RankingRecord>> GetLeaderboardAsync(int count, Guid? districtId);
    Task<IEnumerable<ApplicationUser>> SearchAsync(string searchTerm, int limit);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
}