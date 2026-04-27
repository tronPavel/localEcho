namespace LocalEcho.Core.Models.Statistics;

public record GlobalCounters(int TotalUsers, int TotalMarkers, int TotalActiveMarkers, int PendingReports);

public record ServiceEfficiency(int ResolvedCount, int InProgressCount, int TotalIssues, double Percentage);

public record CategoryMetric(string Label, string CategoryKey, int Count);

public record DistrictRanking(Guid Id, string Name, int TotalMarkers, double SuccessRate);