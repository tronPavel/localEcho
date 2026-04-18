using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Core.Models;
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
            .Include(m => m.Resolution)
                .ThenInclude(r => r.Images)
            .Include(m => m.Resolution)
                .ThenInclude(r => r.ResolvedByUser) // Ссылка на того, кто решил
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<IEnumerable<Marker>> GetForMapAsync(MarkerFilter filter, CancellationToken ct = default)
    {
        var query = _context.Markers.AsNoTracking();

        // 1. Фильтры по категории и статусу
        if (filter.Category.HasValue) query = query.Where(m => m.Category == filter.Category.Value);
        if (filter.Status.HasValue)   query = query.Where(m => m.Status == filter.Status.Value);

        // 2. Пространственный фильтр (Bounding Box)
        if (filter.HasBoundingBox())
        {
            var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            
            // Envelope в NTS — это прямоугольник (XMin, XMax, YMin, YMax)
            var envelope = new Envelope(
                filter.MinLng!.Value, filter.MaxLng!.Value, 
                filter.MinLat!.Value, filter.MaxLat!.Value);
            
            var boundingBox = factory.ToGeometry(envelope);
            boundingBox.SRID = 4326;

            // PostGIS нативно проверит пересечение любой нашей геометрии с этим квадратом
            query = query.Where(m => m.Location.Intersects(boundingBox));
        }

        query = query.OrderByDescending(m => m.CreatedAt);

        return await query.Take(filter.Limit ?? 1000).ToListAsync(ct);
    }

    public async Task<MarkerDetail?> GetDetailAsync(Guid markerId, Guid? currentUserId, CancellationToken ct = default)
    {
        // Запрос остается почти таким же, т.к. m.Location — это теперь просто объект Geometry
        // который спокойно лежит внутри Marker
        return await _context.Markers
            .AsNoTracking()
            .Include(m => m.Images)
            .Include(m => m.Resolution)
                .ThenInclude(r => r.Images)
            .Include(m => m.Resolution)
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
}
