namespace LocalEcho.Core.Entities;

public record SystemWideAnalytics(
    int TotalMarkers,
    int TotalUsers,          
    int ActiveReportsCount,
    MarkerEfficiency Efficiency,
    List<CategoryStat> Categories,
    List<DistrictUsageStat> Districts
);

public record MarkerEfficiency(int ResolvedCount, int OpenIssuesCount, double Percentage);

public record CategoryStat(string CategoryName, int Count);

public record DistrictUsageStat(Guid Id, string Name, int MarkersCount, double ResolvedRate);