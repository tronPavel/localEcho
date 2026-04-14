using LocalEcho.Application.Dtos;

namespace LocalEcho.Application.Interfaces;

public interface IDistrictService
{
    Task<IEnumerable<DistrictDto>> GetAllActiveDistrictsAsync();
    Task<DistrictDto?> GetDistrictByCoordsAsync(double lat, double lng);
    
}