using System.Security.Claims;
using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/moderation")]
[Authorize(Roles = "Moderator,Admin")]
public class ModerationController : ControllerBase
{
    private readonly IModerationService _service;

    public ModerationController(IModerationService service) => _service = service;

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports() 
        => Ok(await _service.GetReportsAsync());

    [HttpPost("markers/{markerId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid markerId)
    {
        await _service.ApproveMarkerAsync(markerId);
        return Ok(new { success = true, message = "Жалобы отклонены, метка восстановлена." });
    }

    [HttpDelete("markers/{markerId:guid}")]
    public async Task<IActionResult> Dismiss(Guid markerId)
    {
        var moderatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.DismissMarkerAsync(markerId, moderatorId);
        return NoContent();
    }
}