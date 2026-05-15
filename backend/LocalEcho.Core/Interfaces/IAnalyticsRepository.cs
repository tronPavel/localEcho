using LocalEcho.Core.Models.Statistics;

namespace LocalEcho.Core.Interfaces;

public interface IAnalyticsRepository
{
    Task<GlobalCounters> GetGlobalCountersAsync(Guid? cityId, CancellationToken ct);
    Task<ServiceEfficiency> GetServiceEfficiencyAsync(Guid? cityId, CancellationToken ct);
    Task<IEnumerable<CategoryMetric>> GetCategoryDistributionAsync(Guid? cityId, CancellationToken ct);
    Task<IEnumerable<DistrictRanking>> GetDistrictRankingAsync(Guid? cityId, int top, CancellationToken ct);
}