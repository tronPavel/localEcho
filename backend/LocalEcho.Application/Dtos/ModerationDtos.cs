namespace LocalEcho.Application.Dtos;

public record CreateReportDto(
    ReportReason Reason,
    string? Comment
);

public record ReportListItemDto(
    Guid Id,
    Guid MarkerId,
    string MarkerTitle,
    string ReporterName,
    ReportReason Reason,
    string? Comment,
    DateTime CreatedAt
);