public enum ReportReason { Spam, Offense, Inaccurate, Fake, Other }

public class Report {
    public Guid Id { get; private set; }
    public Guid MarkerId { get; private set; }
    public Guid ReporterId { get; private set; }
    public ReportReason Reason { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsResolved { get; private set; } 

    private Report() { }
    public Report(Guid markerId, Guid reporterId, ReportReason reason, string? comment) {
        Id = Guid.NewGuid();
        MarkerId = markerId;
        ReporterId = reporterId;
        Reason = reason;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
        IsResolved = false;
    }
    public void Resolve() => IsResolved = true;
}