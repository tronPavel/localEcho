using LocalEcho.Core.Models.Statistics;

namespace LocalEcho.Application.Dtos;

public record GlobalAnalyticsDto(
    GlobalCounters Counters,
    ServiceEfficiency Efficiency,
    IEnumerable<CategoryMetric> CategoryBreakdown,
    IEnumerable<DistrictRanking> TopDistricts
);