using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/statistics")]
[AllowAnonymous]
public class StatisticsController : ControllerBase
{
    private readonly IAnalyticsService _service;
    public StatisticsController(IAnalyticsService service) => _service = service;

    [HttpGet("city-pulse")]
    public async Task<IActionResult> GetCityPulse([FromQuery] Guid? cityId, CancellationToken ct)
    {
        var data = await _service.GetFullCityStatsAsync(cityId, ct);
        return Ok(new { success = true, data });
    }
}