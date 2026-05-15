using LocalEcho.Application.Dtos;

namespace LocalEcho.Aplication.Interfaces;

public interface ICityService
{
    Task<IEnumerable<CityBriefDto>> GetListAsync();
    Task<Guid> CreateAsync(CreateCityDto dto);
    Task UpdateAsync(Guid id, CreateCityDto dto);
}