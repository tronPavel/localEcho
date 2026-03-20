using LocalEcho.Application.Dtos;
using LocalEcho.Core.Entities;

namespace LocalEcho.Application.Interfaces;

public interface IMarkerService
{
    Task<Guid> CreateMarkerAsync(CreateMarkerDto dto, Guid userId, Guid districtId);
    
    Task<IEnumerable<MarkerMapDto>> GetMapMarkersAsync(GetMarkersQueryParams queryParams);
    
    Task<MarkerDetailDto> GetMarkerDetailsAsync(Guid id, Guid? currentUserId);
    
    Task UpdateDescriptionAsync(Guid id, UpdateDescriptionDto dto);
    Task ChangeStatusAsync(Guid id, MarkerStatus newStatus);
    
    Task VoteAsync(Guid markerId, VoteDto dto, Guid voterId);
}