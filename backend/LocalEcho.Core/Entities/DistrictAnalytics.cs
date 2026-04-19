namespace LocalEcho.Core.Entities;

public record DistrictAnalytics(
    int TotalMarkers,
    int ResidentsCount,
    int ResolvedIssuesCount,
    int TotalIssuesCount,
    int OngoingEventsCount,
    int NewSuggestionsCount,
    Dictionary<string, int> CategoryCounts
);