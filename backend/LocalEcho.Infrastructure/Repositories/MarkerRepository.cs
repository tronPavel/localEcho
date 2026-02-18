using LocalEcho.Core.Entities;
using LocalEcho.Core.Models;
using LocalEcho.Infrastructure.Data;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalEcho.Infrastructure.Repositories;

public class MarkerRepository : IMarkerRepository
{
    private readonly AppDbContext _context;

    public MarkerRepository(AppDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Marker?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Markers.FirstOrDefaultAsync(m => m.Id == id, ct);

    //public async Task<IEnumerable<Marker>> GetAllAsync(CancellationToken ct = default)
    //  => await _context.Markers.AsNoTracking().ToListAsync(ct);
    // обновить GetAllAsync чтобы он мог подгружать голоса если нужно лили мы будем делать это отдельным запросом в сервисе.

    public async Task AddAsync(Marker marker, CancellationToken ct = default)
        => await _context.Markers.AddAsync(marker, ct);

    public void Update(Marker marker)
        => _context.Markers.Update(marker);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
    
    public async Task<Vote?> GetVoteAsync(Guid markerId, Guid userId)
        => await _context.Votes.FindAsync(markerId, userId);

    public async Task AddVoteAsync(Vote vote)
        => await _context.Votes.AddAsync(vote);

    public void RemoveVote(Vote vote)
        => _context.Votes.Remove(vote);

    public async Task<int> CalculateRatingAsync(Guid markerId)
    {
        var up = await _context.Votes.CountAsync(v => v.MarkerId == markerId && v.IsUpvote);
        var down = await _context.Votes.CountAsync(v => v.MarkerId == markerId && !v.IsUpvote);
        return up - down;
    }
    public async Task<IEnumerable<MarkerWithVote>> GetFilteredAsync(MarkerFilter filter, Guid? currentUserId, CancellationToken ct = default)
    {
        var query = _context.Markers.AsNoTracking();

        if (filter.Category.HasValue)
            query = query.Where(m => m.Category == filter.Category.Value);

        if (filter.Status.HasValue)
            query = query.Where(m => m.Status == filter.Status.Value);

        if (filter.MinLat.HasValue && filter.MaxLat.HasValue && 
            filter.MinLng.HasValue && filter.MaxLng.HasValue)
        {
            query = query.Where(m => 
                m.Location.Latitude >= filter.MinLat.Value &&
                m.Location.Latitude <= filter.MaxLat.Value &&
                m.Location.Longitude >= filter.MinLng.Value &&
                m.Location.Longitude <= filter.MaxLng.Value);
        }

        return await query.Select(m => new MarkerWithVote(
            m,
            currentUserId.HasValue 
                ? _context.Votes
                    .Where(v => v.MarkerId == m.Id && v.UserId == currentUserId)
                    .Select(v => v.IsUpvote ? 1 : -1)
                    .FirstOrDefault() 
                : 0
        )).ToListAsync(ct);
    }
}