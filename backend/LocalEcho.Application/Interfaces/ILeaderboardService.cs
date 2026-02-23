using LocalEcho.Application.Dtos;

namespace LocalEcho.Application.Interfaces;

public interface ILeaderboardService
{
    Task<IEnumerable<LeaderboardEntryDto>> GetTopUsersAsync(Guid? districtId);
}