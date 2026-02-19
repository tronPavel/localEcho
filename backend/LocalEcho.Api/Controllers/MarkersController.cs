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
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpPost]
    [Authorize(Policy = "User")]
    public async Task<IActionResult> Create([FromBody] CreateMarkerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var markerId = await _service.CreateMarkerAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = markerId }, null);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try 
        {
            var detailDto = await _service.GetMarkerDetailsAsync(id);
            return Ok(detailDto);
        } 
        catch (KeyNotFoundException) 
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetFiltered([FromQuery] GetMarkersQueryParams query)
    {
        var markers = await _service.GetMapMarkersAsync(query);
        return Ok(markers);
    }

    [HttpPost("{id:guid}/vote")]
    [Authorize]
    public async Task<IActionResult> Vote(Guid id, [FromBody] VoteDto dto)
    {
        await _service.VoteAsync(id, dto);
        return Ok(new { message = "Voted successfully" });
    }
}