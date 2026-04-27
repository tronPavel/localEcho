using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Infrastructure.Data;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Repositories;

public class DistrictRepository : IDistrictRepository
{
    private readonly AppDbContext _context;

    public DistrictRepository(AppDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<District?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Districts.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IEnumerable<District>> GetAllActiveAsync(CancellationToken ct = default)
        => await _context.Districts.Where(d => d.IsActive).ToListAsync(ct);

    public async Task<string?> GetNameByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Districts.Where(d => d.Id == id).Select(d => d.Name).FirstOrDefaultAsync(ct);

    public async Task AddAsync(District district, CancellationToken ct = default)
        => await _context.Districts.AddAsync(district, ct);
    public async Task<District?> GetDistrictByCoordinatesAsync(Point p, CancellationToken ct = default)
    {
        return await _context.Districts
            .FirstOrDefaultAsync(d => d.IsActive && d.Boundaries.Contains(p), ct);
    }
    public async Task<bool> IsPointInDistrictAsync(Point p, Guid districtId)
    {
        return await _context.Districts
            .AnyAsync(d => d.Id == districtId && d.Boundaries.Contains(p));
    }
    public async Task<bool> IsOverlappingOtherDistrictsAsync(Guid districtId, Polygon boundaries)
    {
        return await _context.Districts
            .AsNoTracking()
            .Where(d => d.Id != districtId && d.IsActive)
            .AnyAsync(d => d.Boundaries.Relate(boundaries, "T********")); 
    }
    public async Task<DistrictAnalytics> GetAnalyticsAsync(Guid districtId, CancellationToken ct = default)
    {
        var rawStats = await _context.Markers
            .AsNoTracking()
            .Where(m => m.DistrictId == districtId)
            .GroupBy(m => new { m.Category, m.Status })
            .Select(g => new { 
                Category = g.Key.Category, 
                Status = g.Key.Status, 
                Count = g.Count() 
            })
            .ToListAsync(ct);

        int residents = await _context.Users.CountAsync(u => u.DistrictId == districtId, ct);

        return new DistrictAnalytics(
            TotalMarkers: rawStats.Sum(x => x.Count),
            ResidentsCount: residents,
            ResolvedIssuesCount: rawStats.Where(x => x.Category == MarkerCategory.Issue && x.Status == MarkerStatus.Resolved).Sum(x => x.Count),
            TotalIssuesCount: rawStats.Where(x => x.Category == MarkerCategory.Issue).Sum(x => x.Count),
            OngoingEventsCount: rawStats.Where(x => x.Category == MarkerCategory.Event && x.Status == MarkerStatus.Ongoing).Sum(x => x.Count),
            NewSuggestionsCount: rawStats.Where(x => x.Category == MarkerCategory.Suggestion && x.Status == MarkerStatus.Review).Sum(x => x.Count),
            CategoryCounts: rawStats.GroupBy(x => x.Category.ToString()).ToDictionary(g => g.Key, g => g.Sum(x => x.Count))
        );
    }
}