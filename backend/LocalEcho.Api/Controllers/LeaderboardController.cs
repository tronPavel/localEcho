using LocalEcho.Application.Dtos;
using LocalEcho.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/leaderboard")]
[AllowAnonymous]   
public class LeaderboardController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public LeaderboardController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? districtId)
    {
        var query = _userManager.Users.AsNoTracking();

        if (districtId.HasValue)
            query = query.Where(u => u.DistrictId == districtId.Value);

        var leaderboard = await query
            .OrderByDescending(u => u.Points)
            .Take(10)
            .Select(u => new LeaderboardEntryDto(
                u.Id,
                u.UserName ?? u.Email ?? "Anonymous",
                u.Points
            ))
            .ToListAsync();

        return Ok(new { success = true, data = leaderboard });
    }
}