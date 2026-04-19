using System.Security.Claims;
using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarkersController : ControllerBase
{
    private readonly IMarkerService _service;

    public MarkersController(IMarkerService service)
    {
        _service = service;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromForm] CreateMarkerDto dto)
    {
        var id = await _service.CreateMarkerAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetFiltered([FromQuery] GetMarkersQueryParams query)
    {
        var markers = await _service.GetMapMarkersAsync(query);
        return Ok(markers);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Пытаемся достать UserId, если юзер залогинен, чтобы показать его голос (UserVote)
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? userId = string.IsNullOrEmpty(userIdStr) ? null : Guid.Parse(userIdStr);

        var detail = await _service.GetMarkerDetailsAsync(id, userId);
        return Ok(detail);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateMarkerDto dto)
    {
        await _service.UpdateMarkerAsync(id, dto, GetUserId());
        return Ok(new { message = "Метка обновлена" });
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromForm] ChangeStatusDto dto)
    {
        await _service.ChangeStatusAsync(id, dto, GetUserId());
        return Ok(new { message = "Статус обновлен" });
    }

    [HttpPost("{id:guid}/vote")]
    [Authorize]
    public async Task<IActionResult> Vote(Guid id, [FromBody] VoteDto dto)
    {
        await _service.VoteAsync(id, dto, GetUserId());
        return Ok(new { message = "Голос учтен" });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteMarkerAsync(id, GetUserId());
        return NoContent();
    }
}