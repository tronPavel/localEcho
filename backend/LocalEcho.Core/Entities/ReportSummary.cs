namespace LocalEcho.Core.Models.Moderation;

public record ReportSummary(
    Guid Id,
    Guid MarkerId,
    string MarkerTitle,
    string ReporterName,
    ReportReason Reason,
    string? Comment,
    DateTime CreatedAt
);