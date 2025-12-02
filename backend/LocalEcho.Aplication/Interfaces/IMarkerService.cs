using LocalEcho.Aplication.Dtos;
using LocalEcho.Core.Entities;


public interface IMarkerService
{
    Task<Guid> CreateMarkerAsync(CreateMarkerDto dto);
    Task<IEnumerable<MarkerDto>> GetAllAsync();           // ← IEnumerable
    Task<MarkerDto> GetByIdAsync(Guid id);
    Task UpdateDescriptionAsync(Guid id, UpdateDescriptionDto dto);
    Task ChangeStatusAsync(Guid id, MarkerStatus status);
}