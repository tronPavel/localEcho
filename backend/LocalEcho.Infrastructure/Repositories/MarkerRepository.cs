using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Core.Models;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Repositories;

public class MarkerRepository : IMarkerRepository
{
    private readonly AppDbContext _context;

    public MarkerRepository(AppDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    // ... (Остальные методы CRUD/Vote без изменений) ...
    public async Task<Marker?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Markers.FirstOrDefaultAsync(m => m.Id == id, ct);
    public async Task AddAsync(Marker marker, CancellationToken ct = default)
        => await _context.Markers.AddAsync(marker, ct);
    public void Update(Marker marker) => _context.Markers.Update(marker);
    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
    public async Task<Vote?> GetVoteAsync(Guid markerId, Guid userId) => await _context.Votes.FindAsync(markerId, userId);
    public async Task AddVoteAsync(Vote vote) => await _context.Votes.AddAsync(vote);
    public void RemoveVote(Vote vote) => _context.Votes.Remove(vote);
    public async Task<int> CalculateRatingAsync(Guid markerId) {
        var up = await _context.Votes.CountAsync(v => v.MarkerId == markerId && v.IsUpvote);
        var down = await _context.Votes.CountAsync(v => v.MarkerId == markerId && !v.IsUpvote);
        return up - down;
    }

    // === ИСПРАВЛЕННЫЙ МЕТОД ПРЕВЬЮ ===
    public async Task<IEnumerable<MarkerPreview>> GetPreviewsAsync(MarkerFilter filter, CancellationToken ct = default)
    {
        var query = _context.Markers.AsNoTracking();

        if (filter.Category.HasValue)
            query = query.Where(m => m.Category == filter.Category.Value);

        if (filter.Status.HasValue)
            query = query.Where(m => m.Status == filter.Status.Value);

        // BBOX фильтрация (используем сырые свойства Point для SQL)
        if (filter.MinLat.HasValue && filter.MaxLat.HasValue && 
            filter.MinLng.HasValue && filter.MaxLng.HasValue)
        {
            query = query.Where(m =>
                EF.Property<Point>(m, "Location").X >= filter.MinLng.Value &&
                EF.Property<Point>(m, "Location").X <= filter.MaxLng.Value &&
                EF.Property<Point>(m, "Location").Y >= filter.MinLat.Value &&
                EF.Property<Point>(m, "Location").Y <= filter.MaxLat.Value);
        }

        // ПРОЕКЦИЯ
        // Важно: Мы используем m.Location. Благодаря ValueConverter в DbContext, 
        // EF сам превратит geometry(Point) из базы в GeoPoint C# класса.
        return await query.Select(m => new MarkerPreview(
            m.Id,
            m.Title,
            m.Location, // <--- Просто берем свойство, конвертер сделает остальное
            m.Category,
            m.Status
        )).ToListAsync(ct);
    }

    // === ИСПРАВЛЕННЫЙ МЕТОД ДЕТАЛЕЙ ===
    public async Task<MarkerDetail?> GetDetailAsync(Guid markerId, Guid? currentUserId, CancellationToken ct = default)
    {
        // Здесь мы просто берем m (Marker), так как ValueConverter сам превратит Point БД в GeoPoint C#
        return await _context.Markers.AsNoTracking()
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
}