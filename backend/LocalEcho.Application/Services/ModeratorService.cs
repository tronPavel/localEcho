using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Interfaces;

public class ModerationService : IModerationService
{
    private readonly IReportRepository _reportRepo;
    private readonly IMarkerRepository _markerRepo;
    private readonly IMarkerService _markerService;
    private readonly IUnitOfWork _unitOfWork;

    public ModerationService(
        IReportRepository reportRepo, 
        IMarkerRepository markerRepo,
        IMarkerService markerService,
        IUnitOfWork unitOfWork)
    {
        _reportRepo = reportRepo;
        _markerRepo = markerRepo;
        _markerService = markerService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ReportListItemDto>> GetReportsAsync()
    {
        var summaries = await _reportRepo.GetActiveReportsAsync();
        return summaries.Select(s => new ReportListItemDto(
            s.Id, s.MarkerId, s.MarkerTitle, s.ReporterName, s.Reason, s.Comment, s.CreatedAt
        ));
    }

    public async Task ApproveMarkerAsync(Guid markerId)
    {
        var marker = await _markerRepo.GetByIdAsync(markerId) 
            ?? throw new KeyNotFoundException("Метка не найдена");

        marker.Restore();

        await _reportRepo.ResolveAllByMarkerIdAsync(markerId);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DismissMarkerAsync(Guid markerId, Guid moderatorId)
    {
        await _markerService.DeleteMarkerAsync(markerId, moderatorId);
    }

    public async Task CloseSingleReportAsync(Guid reportId)
    {
        var report = await _reportRepo.GetByIdAsync(reportId) ?? throw new KeyNotFoundException("Жалоба не найдена");
        report.Resolve();
        _unitOfWork.SaveChangesAsync();
    }
}