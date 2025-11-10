using LocalEcho.Application.Dtos;
using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;

namespace LocalEcho.Application.Interfaces;

public interface IMarkerService
{
    Task CreateMarkerAsync(CreateMarkerDto dto);
    Task<IEnumerable<MarkerDto>> GetAllMarkersAsync(); 
    /*Task<MarkerDto> GetMarkerByIdAsync(Guid id); 
    Task UpdateMarkerStatusAsync(Guid id, MarkerStatus status);
    Task UpdateMarkerDescriptionAsync(Guid id, string? description); */
}