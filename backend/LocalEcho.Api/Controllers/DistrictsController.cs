using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DistrictsController : ControllerBase
{
    private readonly IDistrictService _service;
    public DistrictsController(IDistrictService service) => _service = service;

    [HttpGet] 
    public async Task<IActionResult> GetList([FromQuery] Guid? cityId) 
    {
        if (cityId.HasValue)
        {
            return Ok(await _service.GetByCityAsync(cityId.Value));
        }
        return Ok(await _service.GetListAsync());
    }

    [HttpGet("map")] 
    public async Task<IActionResult> GetForMap() 
        => Ok(await _service.GetForMapAsync());

    [HttpGet("{id:guid}/details")] 
    public async Task<IActionResult> GetDetails(Guid id) 
        => Ok(await _service.GetDetailAsync(id));

    [HttpGet("find")] 
    public async Task<IActionResult> FindByCoords([FromQuery] double lat, [FromQuery] double lng)
    {
        var district = await _service.GetDistrictByCoordsAsync(lat, lng);
        if (district == null) return NotFound("Территория не входит в зоны обслуживания.");
        return Ok(district);
    }
}