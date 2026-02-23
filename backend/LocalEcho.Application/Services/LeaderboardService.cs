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
        var users = await _userRepository.GetTopUsersAsync(10, districtId);
        
        return users.Select(u => new LeaderboardEntryDto(
            u.Id,
            u.Name ?? u.UserName ?? "Anonymous",   
            u.AvatarUrl,
            u.Points
        ));
    }
}