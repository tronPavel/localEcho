using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarkersController : ControllerBase
{
    private readonly IMarkerService _service;

    public MarkersController(IMarkerService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    private Guid? GetCurrentUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(idStr) ? null : Guid.Parse(idStr);
    }

    private Guid GetCurrentDistrictId()
    {
        var districtStr = User.FindFirst("DistrictId")?.Value;
        return string.IsNullOrEmpty(districtStr) ? Guid.Empty : Guid.Parse(districtStr);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromForm] CreateMarkerDto dto) // [FromForm] вместо [FromBody]
    {
        var userId = GetCurrentUserId()!.Value;
        var districtId = GetCurrentDistrictId();

        var markerId = await _service.CreateMarkerAsync(dto, userId, districtId);
        return CreatedAtAction(nameof(GetById), new { id = markerId }, null);
    }
    
    [HttpGet("{id:guid}")]
    [AllowAnonymous] 
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        
        var detailDto = await _service.GetMarkerDetailsAsync(id, userId);
        return Ok(detailDto);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetFiltered([FromQuery] GetMarkersQueryParams query)
    {
        var markers = await _service.GetMapMarkersAsync(query);
        return Ok(markers);
    }[HttpPost("{id:guid}/vote")]
    [Authorize]
    public async Task<IActionResult> Vote(Guid id,[FromBody] VoteDto dto)
    {
        var voterId = GetCurrentUserId()!.Value;
        
        await _service.VoteAsync(id, dto, voterId);
        
        return Ok(new { message = "Голос успешно учтен" });
    }
}