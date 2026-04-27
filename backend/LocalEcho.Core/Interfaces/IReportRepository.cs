
using LocalEcho.Core.Models.Moderation;

namespace LocalEcho.Core.Interfaces;

public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken ct = default);
    Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Update(Report report);
    Task<IEnumerable<ReportSummary>> GetActiveReportsAsync(CancellationToken ct = default);
    Task ResolveAllByMarkerIdAsync(Guid markerId, CancellationToken ct = default);
    Task<int> GetActiveCountForMarkerAsync(Guid markerId, CancellationToken ct = default);
}