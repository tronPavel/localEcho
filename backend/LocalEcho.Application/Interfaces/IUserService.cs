using LocalEcho.Application.Dtos;

namespace LocalEcho.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId); 
    Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task ChangeDistrictAsync(Guid userId, ChangeDistrictDto dto);
    Task UpdateAvatarAsync(Guid userId, string avatarUrl);
        //Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(Guid? districtId); 
}