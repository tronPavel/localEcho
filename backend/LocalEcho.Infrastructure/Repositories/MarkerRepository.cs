using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Core.Models;
using LocalEcho.Core.Models.Marker;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Repositories;

public class MarkerRepository : IMarkerRepository
{
    private readonly AppDbContext _context;

    public MarkerRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }


    public async Task<Marker?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Markers
            .Include(m => m.Images)
            .Include(m => m.Resolutions) 
            .ThenInclude(r => r.Images)
            .Include(m => m.Resolutions)
            .ThenInclude(r => r.ResolvedByUser) 
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }
    public async Task<Marker?> GetByIdBaseAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Markers
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }
    public async Task<MarkerDetail?> GetDetailAsync(Guid markerId, Guid? currentUserId, CancellationToken ct = default)
    {
        return await _context.Markers
            .AsNoTracking()
            .Include(m => m.Images)
            .Include(m => m.Resolutions) 
            .ThenInclude(r => r.Images)
            .Include(m => m.Resolutions)
            .ThenInclude(r => r.ResolvedByUser)
            .Where(m => m.Id == markerId)
            .Select(m => new MarkerDetail(
                m,
                m.Creator,
                currentUserId.HasValue 
                    ? _context.Votes
                        .Where(v => v.MarkerId == m.Id && v.UserId == currentUserId)
                        .Select(v => v.IsUpvote ? 1 : -1)
                        .FirstOrDefault() 
                    : 0
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<Marker>> GetForMapAsync(MarkerFilter filter, CancellationToken ct = default)
    {
        var query = _context.Markers
            .AsNoTracking()
            .Where(m => !m.IsHidden);

        if (filter.Category.HasValue) query = query.Where(m => m.Category == filter.Category.Value);
        if (filter.Status.HasValue)   query = query.Where(m => m.Status == filter.Status.Value);

        if (filter.HasBoundingBox())
        {
            var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            
            var envelope = new Envelope(
                filter.MinLng!.Value, filter.MaxLng!.Value, 
                filter.MinLat!.Value, filter.MaxLat!.Value);
            
            var boundingBox = factory.ToGeometry(envelope);
            boundingBox.SRID = 4326;

            query = query.Where(m => m.Location.Intersects(boundingBox));
        }

        query = query.OrderByDescending(m => m.CreatedAt);

        return await query.Take(filter.Limit ?? 1000).ToListAsync(ct);
    }
    
    public async Task AddAsync(Marker marker, CancellationToken ct = default)
    {
        await _context.Markers.AddAsync(marker, ct);
    }

    public void Update(Marker marker)
    {
        _context.Markers.Update(marker);
    }


    public async Task<Vote?> GetVoteAsync(Guid markerId, Guid userId)
    {
        return await _context.Votes.FindAsync(new object[] { markerId, userId });
    }

    public async Task AddVoteAsync(Vote vote)
    {
        await _context.Votes.AddAsync(vote);
    }

    public void RemoveVote(Vote vote)
    {
        _context.Votes.Remove(vote);
    }
    
    public Task DeleteAsync(Marker marker)
    {
        _context.Markers.Remove(marker);
        return Task.CompletedTask;
    }
    public async Task<IEnumerable<MarkerWorkTask>> GetOfficialTasksAsync(MarkerWorkFilter filter, CancellationToken ct)
    {
        var query = _context.Markers.AsNoTracking().Where(m => !m.IsHidden);

        if (filter.Category.HasValue)
            query = query.Where(m => m.Category == filter.Category.Value);
        else
            query = query.Where(m => m.Category == MarkerCategory.Issue || m.Category == MarkerCategory.Suggestion);

        if (filter.Status.HasValue)
            query = query.Where(m => m.Status == filter.Status.Value);

        if (filter.DistrictId.HasValue)
            query = query.Where(m => m.DistrictId == filter.DistrictId.Value);
        else if (filter.CityId.HasValue) 
            query = query.Where(m => m.CityId == filter.CityId.Value);

        return await query
            .OrderByDescending(m => m.Rating)
            .ThenByDescending(m => m.CreatedAt)
            .Select(m => new MarkerWorkTask(
                m.Id,
                m.Title,
                m.Category.ToString(),
                m.Status.ToString(),
                m.Creator != null ? (m.Creator.Name ?? "Аноним") : "Аноним",
                _context.Districts.Where(d => d.Id == m.DistrictId).Select(d => d.Name).FirstOrDefault() ?? "Вне района",
                m.CreatedAt,
                m.Rating
            ))
            .Take(filter.Limit)
            .ToListAsync(ct);
    }
}
