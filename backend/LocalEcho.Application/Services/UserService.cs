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
    private readonly IFileService _fileService;
    
    public UserService(
        IUserRepository userRepository, 
        IIdentityRepository identityRepository, 
        IDistrictRepository districtRepository,
        IGeocodingService geocodingService,
        IFileService fileService) 
    {
        _userRepository = userRepository;
        _districtRepository = districtRepository;
        _geocodingService = geocodingService;
        _identityRepository = identityRepository;
        _fileService = fileService;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
                   ?? throw new KeyNotFoundException("Пользователь не найден."); 
               
        var roles = await _identityRepository.GetRolesAsync(user);
    
        DistrictBriefDto? districtDto = null;
        if (user.DistrictId.HasValue)
        {
            var districtName = await _districtRepository.GetNameByIdAsync(user.DistrictId.Value);
            if (districtName != null)
            {
                districtDto = new DistrictBriefDto(user.DistrictId.Value, districtName);
            }
        }

        return new UserProfileDto(
            user.Id, 
            user.Email!, 
            user.Name ?? "User", 
            user.AvatarUrl, 
            user.HomeAddress, 
            user.Points, 
            user.CreatedAt, 
            districtDto, 
            roles,
            user.HomeLocation?.Y, 
            user.HomeLocation?.X 
        );
    }

public async Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
{
    var user = await _userRepository.GetByIdAsync(userId) 
               ?? throw new KeyNotFoundException("Пользователь не найден.");

    if (!string.IsNullOrWhiteSpace(dto.Name)) user.Name = dto.Name;
    
    if (dto.DistrictId.HasValue && dto.DistrictId != user.DistrictId)
    {
        _ = await _districtRepository.GetByIdAsync(dto.DistrictId.Value)
                       ?? throw new KeyNotFoundException("Район не найден.");
        user.DistrictId = dto.DistrictId;
    }

    if (!string.IsNullOrEmpty(dto.HomeAddress) && dto.HomeAddress != user.HomeAddress)
    {
        user.HomeAddress = dto.HomeAddress;
        await SyncCoordinatesAsync(user); 
    }

    if (dto.AvatarFile != null)
    {
        var oldAvatarUrl = user.AvatarUrl;
        
        using var stream = dto.AvatarFile.OpenReadStream();
        var newUrl = await _fileService.SaveFileAsync(stream, dto.AvatarFile.FileName, "avatars");

        user.AvatarUrl = newUrl;

        if (!string.IsNullOrEmpty(oldAvatarUrl))
        {
            await _fileService.DeleteFileAsync(oldAvatarUrl);
        }
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
}

