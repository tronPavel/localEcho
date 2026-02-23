using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/leaderboard")]
[AllowAnonymous]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _service;

    public LeaderboardController(ILeaderboardService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? districtId)
    {
        var leaderboard = await _service.GetTopUsersAsync(districtId);
        return Ok(leaderboard);
    }
}