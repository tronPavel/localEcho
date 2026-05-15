using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Core.Models.Statistics;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalEcho.Infrastructure.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AppDbContext _context;
    public AnalyticsRepository(AppDbContext context) => _context = context;

    public async Task<GlobalCounters> GetGlobalCountersAsync(Guid? cityId, CancellationToken ct)
    {
        var usersQuery = _context.Users.AsNoTracking();
        var markersQuery = _context.Markers.AsNoTracking();
        var reportsQuery = _context.Reports.AsNoTracking().Where(r => !r.IsResolved);

        if (cityId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.CityId == cityId);
            markersQuery = markersQuery.Where(m => m.CityId == cityId);
            reportsQuery = reportsQuery.Join(_context.Markers, 
                r => r.MarkerId, 
                m => m.Id, 
                (r, m) => new { r, m })
                .Where(x => x.m.CityId == cityId)
                .Select(x => x.r);
        }

        return new GlobalCounters(
            TotalUsers: await usersQuery.CountAsync(ct),
            TotalMarkers: await markersQuery.CountAsync(ct),
            TotalActiveMarkers: await markersQuery.CountAsync(m => !m.IsHidden, ct),
            PendingReports: await reportsQuery.CountAsync(ct)
        );
    }

    public async Task<ServiceEfficiency> GetServiceEfficiencyAsync(Guid? cityId, CancellationToken ct)
    {
        var query = _context.Markers
            .AsNoTracking()
            .Where(m => m.Category == MarkerCategory.Issue && !m.IsHidden);

        if (cityId.HasValue)
        {
            query = query.Where(m => m.CityId == cityId);
        }

        var issues = await query.Select(m => m.Status).ToListAsync(ct);

        int total = issues.Count;
        int resolved = issues.Count(s => s == MarkerStatus.Resolved);
        int inProgress = issues.Count(s => s == MarkerStatus.InProgress);

        return new ServiceEfficiency(
            ResolvedCount: resolved, 
            InProgressCount: inProgress, 
            TotalIssues: total, 
            Percentage: total > 0 ? (double)resolved / total * 100 : 100
        );
    }

    public async Task<IEnumerable<CategoryMetric>> GetCategoryDistributionAsync(Guid? cityId, CancellationToken ct)
    {
        var query = _context.Markers.AsNoTracking().Where(m => !m.IsHidden);

        if (cityId.HasValue)
        {
            query = query.Where(m => m.CityId == cityId);
        }

        return await query
            .GroupBy(m => m.Category)
            .Select(g => new CategoryMetric(
                g.Key.ToString(), 
                g.Key.ToString(), 
                g.Count()
            ))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<DistrictRanking>> GetDistrictRankingAsync(Guid? cityId, int top, CancellationToken ct)
    {
        var districtQuery = _context.Districts.AsNoTracking().Where(d => d.IsActive);
        
        if (cityId.HasValue)
        {
            districtQuery = districtQuery.Where(d => d.CityId == cityId);
        }

        var districts = await districtQuery
            .Select(d => new { d.Id, d.Name })
            .ToListAsync(ct);

        var result = new List<DistrictRanking>();

        foreach (var d in districts)
        {
            var markersQuery = _context.Markers.AsNoTracking().Where(m => m.DistrictId == d.Id && !m.IsHidden);
        
            int total = await markersQuery.CountAsync(ct);
        
            var issuesQuery = markersQuery.Where(m => m.Category == MarkerCategory.Issue);
            int totalIssues = await issuesQuery.CountAsync(ct);
        
            int resolvedIssues = await issuesQuery
                .Where(m => m.Status == MarkerStatus.Resolved)
                .CountAsync(ct);
        
            double rate = totalIssues > 0 ? (double)resolvedIssues / totalIssues * 100 : 100;
        
            result.Add(new DistrictRanking(d.Id, d.Name, total, rate));
        }

        return result.OrderByDescending(r => r.TotalMarkers).Take(top).ToList();
    }
}