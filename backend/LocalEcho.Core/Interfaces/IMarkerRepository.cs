using LocalEcho.Core.Entities;
using LocalEcho.Core.Models;

namespace LocalEcho.Core.Interfaces;

public interface IMarkerRepository
{
    Task<Marker?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Marker marker, CancellationToken ct = default);
    void Update(Marker marker);
    
    Task<Vote?> GetVoteAsync(Guid markerId, Guid userId);
    Task AddVoteAsync(Vote vote);
    void RemoveVote(Vote vote);
    
    Task<IEnumerable<MarkerPreview>> GetPreviewsAsync(MarkerFilter filter, CancellationToken ct = default);
    Task<MarkerDetail?> GetDetailAsync(Guid markerId, Guid? currentUserId, CancellationToken ct = default);
}