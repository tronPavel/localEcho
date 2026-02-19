using LocalEcho.Application.Dtos;
using LocalEcho.Core.Entities;

namespace LocalEcho.Application.Interfaces;

public interface IMarkerService
{
    Task<Guid> CreateMarkerAsync(CreateMarkerDto dto);
    Task VoteAsync(Guid markerId, VoteDto dto);
    Task UpdateDescriptionAsync(Guid id, UpdateDescriptionDto dto);
    Task ChangeStatusAsync(Guid id, MarkerStatus newStatus);

    Task<IEnumerable<MarkerMapDto>> GetMapMarkersAsync(GetMarkersQueryParams query);
    Task<MarkerDetailDto> GetMarkerDetailsAsync(Guid id);
}