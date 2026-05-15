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
    private readonly ICityRepository _cityRepository;
    
    public UserService(
        IUserRepository userRepository, 
        IIdentityRepository identityRepository, 
        IDistrictRepository districtRepository,
        IGeocodingService geocodingService,
        IFileService fileService,
        ICityRepository cityRepository) 
    {
        _userRepository = userRepository;
        _districtRepository = districtRepository;
        _geocodingService = geocodingService;
        _identityRepository = identityRepository;
        _fileService = fileService;
        _cityRepository = cityRepository;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
                   ?? throw new KeyNotFoundException("Пользователь не найден."); 
               
        var roles = await _identityRepository.GetRolesAsync(user);

        CityBriefDto? cityDto = null;
        if (user.CityId.HasValue)
        {
            var city = await _cityRepository.GetByIdAsync(user.CityId.Value);
            if (city != null)
            {
                cityDto = new CityBriefDto(city.Id, city.Name, 0, 0); 
            }
        }

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
            user.Id, user.Email!, user.Name ?? "User", user.Bio, user.AvatarUrl,
            user.HomeAddress, user.Points, user.CreatedAt,
            cityDto, districtDto, roles,
            user.HomeLocation?.Y, user.HomeLocation?.X 
        );
    }

  public async Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
{
    var user = await _userRepository.GetByIdAsync(userId) 
               ?? throw new KeyNotFoundException("Пользователь не найден.");

    if (!string.IsNullOrWhiteSpace(dto.Name)) user.Name = dto.Name;
    user.Bio = dto.Bio;

    Guid? targetCityId = dto.CityId.HasValue ? dto.CityId.Value : user.CityId;

    if (dto.CityId.HasValue && dto.CityId != user.CityId)
    {
        var cityExists = await _cityRepository.GetByIdAsync(dto.CityId.Value);
        if (cityExists == null) throw new BadRequestException("Выбранный город не существует.");
        
        user.CityId = dto.CityId;
        user.DistrictId = null; 
    }

    if (dto.DistrictId.HasValue)
    {
        var district = await _districtRepository.GetByIdAsync(dto.DistrictId.Value);
        if (district == null) throw new BadRequestException("Выбранный район не найден.");

        if (district.CityId != targetCityId)
        {
            throw new BadRequestException($"Район '{district.Name}' не входит в границы выбранного города.");
        }

        user.DistrictId = dto.DistrictId;
    }
    else if (dto.CityId.HasValue && !dto.DistrictId.HasValue)
    {
        user.DistrictId = null;
    }

    if (user.HomeAddress != dto.HomeAddress)
    {
        user.HomeAddress = dto.HomeAddress;
        await SyncCoordinatesAsync(user); 
    }

    if (dto.AvatarFile != null)
    {
        using var stream = dto.AvatarFile.OpenReadStream();
        var url = await _fileService.SaveFileAsync(stream, dto.AvatarFile.FileName, "avatars");
        if (!string.IsNullOrEmpty(user.AvatarUrl)) await _fileService.DeleteFileAsync(user.AvatarUrl);
        user.AvatarUrl = url;
    }

    await _userRepository.UpdateAsync(user);
}
private async Task SyncCoordinatesAsync(ApplicationUser user)
{
    if (string.IsNullOrWhiteSpace(user.HomeAddress))
    {
        user.HomeLocation = null;
        return;
    }

    var point = await _geocodingService.GetCoordinatesAsync(user.HomeAddress);
    
    if (point == null)
    {
        throw new BadRequestException($"Не удалось найти адрес '{user.HomeAddress}'. Пожалуйста, уточните номер дома или название улицы.");
    }

    if (user.DistrictId.HasValue)
    {
        var isInDistrict = await _districtRepository.IsPointInDistrictAsync(point, user.DistrictId.Value);
        
        if (!isInDistrict)
        {
            throw new BadRequestException("Этот адрес находится в другом районе. Выберите ваш реальный район проживания или исправьте адрес.");
        }
    }
    
    user.HomeLocation = point;
}
    
    public async Task AssignRoleAsync(Guid userId, string roleName)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
                   ?? throw new KeyNotFoundException("Пользователь не найден");

        var allowedRoles = new[] { "Moderator", "Official", "User" };
        if (!allowedRoles.Contains(roleName)) throw new BadRequestException("Недопустимая роль");

        await _identityRepository.AddToRoleAsync(user, roleName);
    }

    public async Task RemoveRoleAsync(Guid userId, string roleName)
    {
        var user = await _userRepository.GetByIdAsync(userId) ?? throw new KeyNotFoundException();
        await _identityRepository.RemoveFromRoleAsync(user, roleName);
    }

    public async Task<IEnumerable<UserProfileDto>> SearchUsersAsync(string query)
    {
        var users = await _userRepository.SearchAsync(query, 20);
    
        var results = new List<UserProfileDto>();
        foreach (var user in users)
        {
            results.Add(await GetProfileAsync(user.Id));
        }
        return results;
    }
}

