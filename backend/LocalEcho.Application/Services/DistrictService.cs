using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Interfaces;
using NetTopologySuite.Geometries;

namespace LocalEcho.Application.Services;

public class DistrictService : IDistrictService
{
    private readonly IDistrictRepository _repository;
    private readonly GeometryFactory _geometryFactory;

    public DistrictService(IDistrictRepository repository, GeometryFactory geometryFactory)
    {
        _repository = repository;
        _geometryFactory = geometryFactory;
    }

    public async Task<DistrictDto?> GetDistrictByCoordsAsync(double lat, double lng)
    {
        var point = _geometryFactory.CreatePoint(new Coordinate(lng, lat));
        var district = await _repository.GetDistrictByCoordinatesAsync(point);

        if (district == null) return null;

        return new DistrictDto(
            district.Id, 
            district.Name, 
            district.Description, 
            district.Centroid.Y, 
            district.Centroid.X,
            district.IconColor
        );
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