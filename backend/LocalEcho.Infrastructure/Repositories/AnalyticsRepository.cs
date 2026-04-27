using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Core.Models.Statistics;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AppDbContext _context;
    public AnalyticsRepository(AppDbContext context) => _context = context;

    public async Task<GlobalCounters> GetGlobalCountersAsync(CancellationToken ct)
    {
        return new GlobalCounters(
            await _context.Users.CountAsync(ct),
            await _context.Markers.CountAsync(ct),
            await _context.Markers.CountAsync(m => !m.IsHidden, ct),
            await _context.Reports.CountAsync(r => !r.IsResolved, ct)
        );
    }

    public async Task<ServiceEfficiency> GetServiceEfficiencyAsync(CancellationToken ct)
    {
        var issues = await _context.Markers
            .Where(m => m.Category == MarkerCategory.Issue && !m.IsHidden)
            .Select(m => m.Status)
            .ToListAsync(ct);

        int total = issues.Count;
        int resolved = issues.Count(s => s == MarkerStatus.Resolved);
        int inProgress = issues.Count(s => s == MarkerStatus.InProgress);

        return new ServiceEfficiency(resolved, inProgress, total, total > 0 ? (double)resolved / total * 100 : 100);
    }

    public async Task<IEnumerable<CategoryMetric>> GetCategoryDistributionAsync(CancellationToken ct)
    {
        return await _context.Markers
            .Where(m => !m.IsHidden)
            .GroupBy(m => m.Category)
            .Select(g => new CategoryMetric(g.Key.ToString(), g.Key.ToString(), g.Count()))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<DistrictRanking>> GetDistrictRankingAsync(int top, CancellationToken ct)
    {
        var districts = await _context.Districts
            .Where(d => d.IsActive)
            .Select(d => new { d.Id, d.Name })
            .ToListAsync(ct);

        var result = new List<DistrictRanking>();

        foreach (var d in districts)
        {
            var markersQuery = _context.Markers.Where(m => m.DistrictId == d.Id && !m.IsHidden);
        
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