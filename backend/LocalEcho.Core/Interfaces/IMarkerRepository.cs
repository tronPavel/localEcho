
using LocalEcho.Core.Entities;
using LocalEcho.Core.Models;

public interface IMarkerRepository
{
    Task<Marker?> GetByIdAsync(Guid id, CancellationToken ct = default);
    //Task<IEnumerable<Marker>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Marker marker, CancellationToken ct = default);
    void Update(Marker marker);
    Task SaveChangesAsync(CancellationToken ct = default); 
    Task<Vote?> GetVoteAsync(Guid markerId, Guid userId);
    Task AddVoteAsync(Vote vote);
    void RemoveVote(Vote vote);
    Task<int> CalculateRatingAsync(Guid markerId);
    Task<IEnumerable<MarkerWithVote>> GetFilteredAsync(MarkerFilter filter, Guid? currentUserId, CancellationToken ct = default);

}