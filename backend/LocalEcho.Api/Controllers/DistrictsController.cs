using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DistrictsController : ControllerBase
{
    private readonly IDistrictService _service;

    public DistrictsController(IDistrictService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var districts = await _service.GetAllActiveDistrictsAsync();
        return Ok(new { success = true, data = districts });
    }
    
    [HttpGet("find-by-coords")]
    public async Task<IActionResult> GetByCoords([FromQuery] double lat, [FromQuery] double lng)
    {
        var district = await _service.GetDistrictByCoordsAsync(lat, lng);
    
        if (district == null) 
            return NotFound(new { success = false, message = "Район не найден" });
    
        return Ok(new { success = true, data = district });
    }
}