using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarkersController : ControllerBase
{
    private readonly IMarkerService _service;

    public MarkersController(IMarkerService service)
    {
        _service = service; //?? throw new ArgumentNullException(nameof(service));
    }

    [HttpPost] 
    public async Task<IActionResult> Create([FromBody] CreateMarkerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState); 
        await _service.CreateMarkerAsync(dto); 
        return Ok(); 
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var markers = await _service.GetAllMarkersAsync();
        return Ok(markers);
    }

    // Аналогично для GetById, UpdateStatus (PUT/PATCH).
}