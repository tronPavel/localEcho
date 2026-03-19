using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Interfaces;

namespace LocalEcho.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityRepository _identityRepository; // ДОБАВЛЕНО
    private readonly IDistrictRepository _districtRepository;

    public UserService(
        IUserRepository userRepository, 
        IIdentityRepository identityRepository, 
        IDistrictRepository districtRepository)
    {
        _userRepository = userRepository;
        _identityRepository = identityRepository;
        _districtRepository = districtRepository;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
                   ?? throw new Exception("User not found");
        
        var roles = await _identityRepository.GetRolesAsync(user);
        var district = await _districtRepository.GetByIdAsync(user.DistrictId ?? Guid.Empty);
        
        DistrictDto? districtDto = null;
        if (district != null)
        {
            districtDto = new DistrictDto(district.Id, district.Name, district.Description, district.CenterLat, district.CenterLng, district.IconColor);
        }

        return new UserProfileDto(
            user.Id, user.Email!, user.Name ?? "User", user.AvatarUrl, user.HomeAddress,
            user.IsVerified, user.Points, user.LastSeen, user.CreatedAt, districtDto, roles
        );
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");
        
        if (dto.Name != null) user.Name = dto.Name; 
        if (dto.HomeAddress != null) user.HomeAddress = dto.HomeAddress;
        
        await _userRepository.UpdateAsync(user);
    }

    public async Task ChangeDistrictAsync(Guid userId, ChangeDistrictDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");
        var district = await _districtRepository.GetByIdAsync(dto.DistrictId) ?? throw new Exception("District not found");

        user.DistrictId = dto.DistrictId;
        user.HomeLatitude = district.CenterLat;
        user.HomeLongitude = district.CenterLng;
        if (dto.HomeAddress != null) user.HomeAddress = dto.HomeAddress;

        await _userRepository.UpdateAsync(user);
    }

    public async Task UpdateAvatarAsync(Guid userId, string avatarUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new Exception("User not found");
        user.AvatarUrl = avatarUrl;
        await _userRepository.UpdateAsync(user);
    }
}