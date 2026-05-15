using LocalEcho.Aplication.Interfaces;
using LocalEcho.Application.Dtos;
using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using NetTopologySuite.Geometries;

namespace LocalEcho.Application.Services;
public class CityService : ICityService
{
    private readonly ICityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly GeometryFactory _geometryFactory;

    public CityService(ICityRepository repository, IUnitOfWork unitOfWork, GeometryFactory geometryFactory)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _geometryFactory = geometryFactory;
    }

    public async Task<IEnumerable<CityBriefDto>> GetListAsync()
    {
        var cities = await _repository.GetAllAsync();
        return cities.Select(c => new CityBriefDto(
            c.Id, 
            c.Name, 
            c.Boundaries.Centroid.Y, 
            c.Boundaries.Centroid.X  
        ));
    }

    public async Task<Guid> CreateAsync(CreateCityDto dto)
    {
        var polygon = MapToPolygon(dto.Geometry);
        var city = City.Create(dto.Name, polygon);
        
        await _repository.AddAsync(city);
        await _unitOfWork.SaveChangesAsync();
        return city.Id;
    }

    public async Task UpdateAsync(Guid id, CreateCityDto dto)
    {
        var city = await _repository.GetByIdAsync(id) 
                   ?? throw new KeyNotFoundException("Город не найден");

        var polygon = MapToPolygon(dto.Geometry);
        city.Update(dto.Name, polygon);
        
        await _unitOfWork.SaveChangesAsync();
    }
    

    private Polygon MapToPolygon(List<CoordinateDto> geometryDto)
    {
        var coords = geometryDto.Select(c => new Coordinate(c.Lng, c.Lat)).ToList();
        if (!coords.First().Equals2D(coords.Last())) coords.Add(new Coordinate(coords.First().X, coords.First().Y));
        var poly = _geometryFactory.CreatePolygon(coords.ToArray());
        poly.SRID = 4326;
        return poly;
    }
}