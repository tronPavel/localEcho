using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities.Identity;
using LocalEcho.Core.Exceptions;
using LocalEcho.Core.Interfaces;


namespace LocalEcho.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly IGeocodingService _geocodingService;
    private readonly IIdentityRepository _identityRepository;
    public UserService(
        IUserRepository userRepository, 
        IIdentityRepository identityRepository, 
        IDistrictRepository districtRepository,
        IGeocodingService geocodingService) 
    {
        _userRepository = userRepository;
        _districtRepository = districtRepository;
        _geocodingService = geocodingService;
        _identityRepository = identityRepository;
    }

  public async Task<UserProfileDto> GetProfileAsync(Guid userId)
{
    var user = await _userRepository.GetByIdAsync(userId) 
               ?? throw new KeyNotFoundException("Пользователь не найден."); 
               
    var roles = await _identityRepository.GetRolesAsync(user);
    var district = user.DistrictId.HasValue 
        ? await _districtRepository.GetByIdAsync(user.DistrictId.Value) 
        : null;
    
    DistrictDto? districtDto = district != null ? new DistrictDto(
        district.Id, district.Name, district.Description, 
        district.Centroid.Y, district.Centroid.X, district.IconColor) : null;

    return new UserProfileDto(
        user.Id, user.Email!, user.Name ?? "User", user.AvatarUrl, user.HomeAddress,
        user.IsVerified, user.Points, user.CreatedAt, districtDto, roles,
        user.HomeLocation?.Y, user.HomeLocation?.X // Возвращаем Lat/Lng
    );
}

public async Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
                   ?? throw new KeyNotFoundException("Пользователь не найден.");

        if (dto.Name != null) user.Name = dto.Name;

        if (!string.IsNullOrEmpty(dto.HomeAddress) && dto.HomeAddress != user.HomeAddress)
        {
            user.HomeAddress = dto.HomeAddress;
            await SyncCoordinatesAsync(user);
        }
        
        await _userRepository.UpdateAsync(user);
    }

    public async Task ChangeDistrictAsync(Guid userId, ChangeDistrictDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
                   ?? throw new KeyNotFoundException("Пользователь не найден.");
            
        var district = await _districtRepository.GetByIdAsync(dto.DistrictId) 
                       ?? throw new KeyNotFoundException("Указанный район не существует.");
        
        user.DistrictId = dto.DistrictId;
        
        if (!string.IsNullOrEmpty(user.HomeAddress))
        {
            await SyncCoordinatesAsync(user);
        }

        await _userRepository.UpdateAsync(user);
    }

    private async Task SyncCoordinatesAsync(ApplicationUser user)
    {
        if (string.IsNullOrEmpty(user.HomeAddress)) return;

        var point = await _geocodingService.GetCoordinatesAsync(user.HomeAddress);
        
        if (point != null)
        {
            if (user.DistrictId.HasValue)
            {
                var isInDistrict = await _districtRepository.IsPointInDistrictAsync(point, user.DistrictId.Value);
                if (!isInDistrict)
                {
                    throw new BadRequestException("Указанный адрес не относится к вашему району.");
                }
            }
            
            user.HomeLocation = point;
        }
        else
        {
            user.HomeLocation = null;
        }
    }
    public async Task UpdateAvatarAsync(Guid userId, string avatarUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
                   ?? throw new KeyNotFoundException("Пользователь не найден.");
        user.AvatarUrl = avatarUrl;
        await _userRepository.UpdateAsync(user);
    }
}