using LocalEcho.Core.Models.Statistics;

namespace LocalEcho.Core.Interfaces;

public interface IAnalyticsRepository
{
    Task<GlobalCounters> GetGlobalCountersAsync(CancellationToken ct);
    Task<ServiceEfficiency> GetServiceEfficiencyAsync(CancellationToken ct);
    Task<IEnumerable<CategoryMetric>> GetCategoryDistributionAsync(CancellationToken ct);
    Task<IEnumerable<DistrictRanking>> GetDistrictRankingAsync(int top, CancellationToken ct);
}