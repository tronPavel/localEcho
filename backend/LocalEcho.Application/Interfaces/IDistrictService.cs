using LocalEcho.Application.Dtos;

namespace LocalEcho.Application.Interfaces;

public interface IDistrictService
{
    Task<IEnumerable<DistrictBriefDto>> GetListAsync();
    Task<IEnumerable<DistrictMapDto>> GetForMapAsync();
    Task<DistrictDetailDto> GetDetailAsync(Guid id);
    Task<DistrictBriefDto?> GetDistrictByCoordsAsync(double lat, double lng);
    Task<IEnumerable<DistrictBriefDto>> GetByCityAsync(Guid cityId);
    Task<Guid> CreateAsync(CreateDistrictDto dto);
    Task UpdateAsync(Guid id, CreateDistrictDto dto);
    Task DeleteAsync(Guid id);
}