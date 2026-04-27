using LocalEcho.Application.Dtos;

namespace LocalEcho.Application.Interfaces;

public interface IModerationService
{
    Task<IEnumerable<ReportListItemDto>> GetReportsAsync();
    Task DismissMarkerAsync(Guid markerId, Guid moderatorId);
    Task ApproveMarkerAsync(Guid markerId);
    Task CloseSingleReportAsync(Guid reportId);
    
}