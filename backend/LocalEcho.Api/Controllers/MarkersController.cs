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
        => Ok(await _service.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());
}