using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;

namespace LocalEcho.Application.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly IUserRepository _userRepository;

    public LeaderboardService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetTopUsersAsync(Guid? districtId)
    {
        var records = await _userRepository.GetLeaderboardAsync(10, districtId);

        return records.Select(r => new LeaderboardEntryDto(
            r.UserId, 
            r.Name, 
            r.AvatarUrl, 
            r.Points
        ));
    }
}