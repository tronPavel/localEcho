using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Interfaces;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repo;
    public AnalyticsService(IAnalyticsRepository repo) => _repo = repo;

    public async Task<GlobalAnalyticsDto> GetFullCityStatsAsync(Guid? cityId, CancellationToken ct)
    {
        var counters = await _repo.GetGlobalCountersAsync(cityId, ct);
        var efficiency = await _repo.GetServiceEfficiencyAsync(cityId, ct);
        var categories = await _repo.GetCategoryDistributionAsync(cityId, ct);
        var districts = await _repo.GetDistrictRankingAsync(cityId, 5, ct);

        return new GlobalAnalyticsDto(counters, efficiency, categories, districts);
    }
}