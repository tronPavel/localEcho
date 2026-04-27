using LocalEcho.Core.Interfaces;
using LocalEcho.Core.Models.Moderation;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;
    public ReportRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Report report, CancellationToken ct) => await _context.Reports.AddAsync(report, ct);

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken ct) => await _context.Reports.FindAsync(new object[] { id }, ct);

    public void Update(Report report) => _context.Reports.Update(report);

    public async Task<IEnumerable<ReportSummary>> GetActiveReportsAsync(CancellationToken ct)
    {
        var query = from r in _context.Reports
            where !r.IsResolved
            join m in _context.Markers on r.MarkerId equals m.Id
            join u in _context.Users on r.ReporterId equals u.Id
            select new { 
                r.Id, 
                MarkerId = m.Id, 
                m.Title, 
                ReporterName = u.Name ?? "Аноним", 
                r.Reason, 
                r.Comment, 
                r.CreatedAt 
            };

        var results = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

        return results.Select(x => new ReportSummary(
            x.Id, x.MarkerId, x.Title, x.ReporterName, x.Reason, x.Comment, x.CreatedAt
        ));
    }

    public async Task ResolveAllByMarkerIdAsync(Guid markerId, CancellationToken ct)
    {
        await _context.Reports
            .Where(r => r.MarkerId == markerId && !r.IsResolved)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsResolved, true), ct);
    }
    public async Task<int> GetActiveCountForMarkerAsync(Guid markerId, CancellationToken ct = default)
    {
        return await _context.Reports
            .CountAsync(r => r.MarkerId == markerId && !r.IsResolved, ct);
    }
}