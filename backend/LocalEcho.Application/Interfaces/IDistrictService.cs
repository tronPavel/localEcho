using LocalEcho.Application.Dtos;

namespace LocalEcho.Application.Interfaces;

public interface IDistrictService
{
    Task<IEnumerable<DistrictDto>> GetAllActiveDistrictsAsync();
}