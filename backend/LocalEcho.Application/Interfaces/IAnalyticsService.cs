using LocalEcho.Application.Dtos;

namespace LocalEcho.Application.Interfaces;

public interface IAnalyticsService
{
    Task<GlobalAnalyticsDto> GetFullCityStatsAsync(Guid? cityId, CancellationToken ct);
}