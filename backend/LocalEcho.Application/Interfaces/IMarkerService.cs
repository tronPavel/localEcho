using LocalEcho.Application.Dtos;

namespace LocalEcho.Application.Interfaces;

public interface IMarkerService
{
    Task<Guid> CreateMarkerAsync(CreateMarkerDto dto, Guid userId);
    
    Task<IEnumerable<MarkerMapDto>> GetMapMarkersAsync(GetMarkersQueryParams queryParams);
    
    Task<MarkerDetailDto> GetMarkerDetailsAsync(Guid id, Guid? currentUserId);
    
    Task UpdateMarkerAsync(Guid id, UpdateMarkerDto dto, Guid userId);

    Task ChangeStatusAsync(Guid id, ChangeStatusDto dto, Guid userId);
    
    Task VoteAsync(Guid markerId, VoteDto dto, Guid voterId);
    
    Task DeleteMarkerAsync(Guid id, Guid userId);
}