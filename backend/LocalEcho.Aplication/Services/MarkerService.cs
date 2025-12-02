using LocalEcho.Aplication.Dtos;
using LocalEcho.Core.Entities;


public class MarkerService : IMarkerService
{
    private readonly IMarkerRepository _repository;

    public MarkerService(IMarkerRepository repository)
        => _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<Guid> CreateMarkerAsync(CreateMarkerDto dto)
    {
        var location = new GeoPoint(dto.Latitude, dto.Longitude);
        var marker = Marker.Create(dto.Title, location, dto.Category, dto.Description);

        await _repository.AddAsync(marker);
        await _repository.SaveChangesAsync();

        return marker.Id;
    }

    public async Task<IEnumerable<MarkerDto>> GetAllAsync()
    {
        var markers = await _repository.GetAllAsync();
        return markers.Select(m => new MarkerDto(
            m.Id,
            m.Title,
            m.Location.Latitude,
            m.Location.Longitude,
            m.Description,
            m.Category,
            m.Status,
            m.CreatedAt,
            m.UpdatedAt
        )).ToArray();
    }

    public async Task<MarkerDto> GetByIdAsync(Guid id)
    {
        var marker = await _repository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException($"Marker {id} not found");

        return new MarkerDto(
            marker.Id, marker.Title,
            marker.Location.Latitude, marker.Location.Longitude,
            marker.Description, marker.Category, marker.Status,
            marker.CreatedAt, marker.UpdatedAt);
    }

    // В будущем: добавить Guid currentUserId в сигнатуру или брать из HttpContext
    public async Task UpdateDescriptionAsync(Guid id, UpdateDescriptionDto dto)
    {
        var marker = await _repository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException();

        marker.UpdateDescription(dto.Description);
        _repository.Update(marker);
        await _repository.SaveChangesAsync();
    }

    // В будущем: проверка прав
    public async Task ChangeStatusAsync(Guid id, MarkerStatus newStatus)
    {
        var marker = await _repository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException();

        marker.ChangeStatus(newStatus);
        _repository.Update(marker);
        await _repository.SaveChangesAsync();
    }
}