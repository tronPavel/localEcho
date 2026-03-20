using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Interfaces;

namespace LocalEcho.Application.Services;

public class DistrictService : IDistrictService
{
    private readonly IDistrictRepository _repository;

    public DistrictService(IDistrictRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<DistrictDto>> GetAllActiveDistrictsAsync()
    {
        var districts = await _repository.GetAllActiveAsync();
        
        return districts.Select(d => new DistrictDto(
            d.Id, 
            d.Name, 
            d.Description, 
            d.Centroid.Y, 
            d.Centroid.X,
            d.IconColor
        ));
    }
}