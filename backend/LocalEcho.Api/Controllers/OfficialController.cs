using LocalEcho.Application.Dtos;
using LocalEcho.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/official")]
[Authorize(Roles = "Official,Admin,Moderator")]
public class OfficialController : ControllerBase
{
    private readonly IOfficialService _service;
    public OfficialController(IOfficialService service) => _service = service;

    [HttpGet("queue")]
    public async Task<IActionResult> GetWorkQueue([FromQuery] WorkItemsQueryParams query, CancellationToken ct)
    {
        var result = await _service.GetQueueAsync(query, ct);
        return Ok(result);
    }
}