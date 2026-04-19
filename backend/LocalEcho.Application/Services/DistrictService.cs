using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using NetTopologySuite.Geometries;

namespace LocalEcho.Application.Services;

public class DistrictService : IDistrictService
{
    private readonly IDistrictRepository _repository;
    private readonly GeometryFactory _geometryFactory;
    private readonly IUnitOfWork _unitOfWork;

    public DistrictService(IDistrictRepository repository, GeometryFactory geometryFactory, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _geometryFactory = geometryFactory;
        _unitOfWork = unitOfWork;
    }

    // 1. Краткий список для регистрации и фильтров
    public async Task<IEnumerable<DistrictBriefDto>> GetListAsync()
    {
        var districts = await _repository.GetAllActiveAsync();
        return districts.Select(d => new DistrictBriefDto(d.Id, d.Name));
    }

    // 2. Детальная информация для модалки района с аналитикой
    public async Task<DistrictDetailDto> GetDetailAsync(Guid id)
    {
        var district = await _repository.GetByIdAsync(id) 
                       ?? throw new KeyNotFoundException("Район не найден.");
    
        var analytics = await _repository.GetAnalyticsAsync(id);

        var statsDto = new DistrictStatsDto(
            analytics.TotalMarkers,
            analytics.ResidentsCount,
            analytics.ResolvedIssuesCount,
            analytics.TotalIssuesCount > 0 
                ? Math.Round((double)analytics.ResolvedIssuesCount / analytics.TotalIssuesCount * 100, 1) 
                : 100,
            analytics.OngoingEventsCount,
            analytics.NewSuggestionsCount,
            analytics.CategoryCounts
        );

        return new DistrictDetailDto(district.Id, district.Name, district.Description, statsDto);
    }

    // 3. Данные для отрисовки полигонов на карте
    public async Task<IEnumerable<DistrictMapDto>> GetForMapAsync()
    {
        var districts = await _repository.GetAllActiveAsync();
        return districts.Select(d => new DistrictMapDto(
            d.Id, 
            d.Name, 
            // Превращаем координаты NTS в наш DTO [Y = Lat, X = Lng]
            d.Boundaries.Coordinates.Select(c => new CoordinateDto(c.Y, c.X)).ToList(),
            new CoordinateDto(d.Centroid.Y, d.Centroid.X)
        ));
    }

    // 4. Определение района по точке (Reverse Geocoding)
    public async Task<DistrictBriefDto?> GetDistrictByCoordsAsync(double lat, double lng)
    {
        var point = _geometryFactory.CreatePoint(new Coordinate(lng, lat));
        point.SRID = 4326;
    
        var district = await _repository.GetDistrictByCoordinatesAsync(point);
        if (district == null) return null;

        return new DistrictBriefDto(district.Id, district.Name);
    }

    // --- ADMIN METHODS ---

    public async Task<Guid> CreateAsync(CreateDistrictDto dto)
    {
        var coords = dto.Geometry.Select(c => new Coordinate(c.Lng, c.Lat)).ToList();
        
        // PostGIS/NTS требуют, чтобы полигон был замкнут (первая точка == последняя)
        if (!coords.First().Equals2D(coords.Last())) 
            coords.Add(new Coordinate(coords.First().X, coords.First().Y));

        var polygon = _geometryFactory.CreatePolygon(coords.ToArray());
        polygon.SRID = 4326;

        var district = District.Create(dto.Name, polygon, dto.Description);
        await _repository.AddAsync(district);
        await _unitOfWork.SaveChangesAsync();
        return district.Id;
    }

    public async Task UpdateAsync(Guid id, CreateDistrictDto dto)
    {
        var district = await _repository.GetByIdAsync(id) 
                       ?? throw new KeyNotFoundException("Район для обновления не найден.");
        
        district.Update(dto.Name, dto.Description, district.IsActive);
        
        var coords = dto.Geometry.Select(c => new Coordinate(c.Lng, c.Lat)).ToList();
        if (!coords.First().Equals2D(coords.Last())) 
            coords.Add(new Coordinate(coords.First().X, coords.First().Y));
        
        var polygon = _geometryFactory.CreatePolygon(coords.ToArray());
        polygon.SRID = 4326;
        
        district.UpdateGeometry(polygon);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var district = await _repository.GetByIdAsync(id) 
                       ?? throw new KeyNotFoundException("Район не найден.");
        
        // В нашем случае используем деактивацию вместо физического удаления, 
        // чтобы не "сломать" созданные ранее в этом районе метки.
        district.SetActive(false);
        await _unitOfWork.SaveChangesAsync();
    }
}