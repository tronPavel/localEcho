namespace LocalEcho.Core.Entities;

public record ReportListItem(
    Guid Id,
    Guid MarkerId,
    string MarkerTitle,
    string ReporterName,
    ReportReason Reason,
    string? Comment,
    DateTime CreatedAt
);