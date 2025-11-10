using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;

namespace LocalEcho.Application.Services;

public class MarkerService : IMarkerService
{
    private readonly IMarkerRepository _repository; 

    public MarkerService(IMarkerRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task CreateMarkerAsync(CreateMarkerDto dto)
    {
        var location = new GeoPoint(dto.Latitude, dto.Longitude);
        var marker = new Marker(dto.Title, location, dto.Category, dto.Description);
        await _repository.AddAsync(marker);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<MarkerDto>> GetAllMarkersAsync()
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
        ));
    }

    /*public async Task<MarkerDto> GetMarkerByIdAsync(Guid id)
    {
        var marker = await _repository.GetByIdAsync(id);
        if (marker == null) throw new KeyNotFoundException("Marker not found");
        return new MarkerDto(marker.Id, marker.Title, marker.Location.Latitude, marker.Location.Longitude, marker.Description, marker.Category, marker.Status, marker.CreatedAt, marker.UpdatedAt);
    }

    public async Task UpdateMarkerStatusAsync(Guid id, MarkerStatus status)
    {
        var marker = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Marker not found");
        marker.ChangeStatus(status);
        await _repository.UpdateAsync(marker);
        await _repository.SaveChangesAsync();
    }

    public async Task UpdateMarkerDescriptionAsync(Guid id, string? description)
    {
        var marker = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Marker not found");
        marker.UpdateDescription(description);
        await _repository.UpdateAsync(marker);
        await _repository.SaveChangesAsync();
    }*/
}