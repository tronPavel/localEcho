using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;

namespace LocalEcho.Application.Services;

public class MarkerService : IMarkerService
{
    private readonly IMarkerRepository _repository;
    private readonly IUserContext _userContext;

    public MarkerService(IMarkerRepository repository, IUserContext userContext)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }

    public async Task<Guid> CreateMarkerAsync(CreateMarkerDto dto)
    {
        if (!_userContext.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated");

        var userId = _userContext.UserId;
        var districtId = _userContext.DistrictId;

        var location = new GeoPoint(dto.Latitude, dto.Longitude);
        
        var marker = Marker.Create(
            dto.Title, 
            location, 
            dto.Category, 
            userId, 
            districtId, 
            dto.Description
        );

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
            m.CreatedByUserId,
            m.CreatedAt,
            m.UpdatedAt
        ));
    }

    public async Task<MarkerDto> GetByIdAsync(Guid id)
    {
        var marker = await _repository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException($"Marker {id} not found");

        return new MarkerDto(
            marker.Id, 
            marker.Title,
            marker.Location.Latitude, 
            marker.Location.Longitude,
            marker.Description, 
            marker.Category, 
            marker.Status,
            marker.CreatedByUserId,
            marker.CreatedAt, 
            marker.UpdatedAt
        );
    }

    public async Task UpdateDescriptionAsync(Guid id, UpdateDescriptionDto dto)
    {
        // if (_userContext.UserId != marker.CreatedByUserId) throw ...
        
        var marker = await _repository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException();

        marker.UpdateDescription(dto.Description);
        _repository.Update(marker);
        await _repository.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(Guid id, MarkerStatus newStatus)
    {
        var marker = await _repository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException();

        marker.ChangeStatus(newStatus);
        _repository.Update(marker);
        await _repository.SaveChangesAsync();
    }
}