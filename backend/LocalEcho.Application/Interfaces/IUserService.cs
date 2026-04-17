using LocalEcho.Application.Dtos;

namespace LocalEcho.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId); 
    Task UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
}