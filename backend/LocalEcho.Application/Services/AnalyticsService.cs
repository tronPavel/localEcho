using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Interfaces;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repo;
    public AnalyticsService(IAnalyticsRepository repo) => _repo = repo;

    public async Task<GlobalAnalyticsDto> GetFullCityStatsAsync(CancellationToken ct)
    {
        return new GlobalAnalyticsDto(
            await _repo.GetGlobalCountersAsync(ct),
            await _repo.GetServiceEfficiencyAsync(ct),
            await _repo.GetCategoryDistributionAsync(ct),
            await _repo.GetDistrictRankingAsync(5, ct)
        );
    }
}