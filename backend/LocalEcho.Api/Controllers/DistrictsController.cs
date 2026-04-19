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

    [HttpGet] // Краткий список для Select-ов
    public async Task<IActionResult> GetList() 
        => Ok(await _service.GetListAsync());

    [HttpGet("map")] // Данные для отрисовки слоев на карте
    public async Task<IActionResult> GetForMap() 
        => Ok(await _service.GetForMapAsync());

    [HttpGet("{id:guid}/details")] // Статистика для модалки района
    public async Task<IActionResult> GetDetails(Guid id) 
        => Ok(await _service.GetDetailAsync(id));

    [HttpGet("find")] // Reverse geocoding (где я нахожусь?)
    public async Task<IActionResult> FindByCoords([FromQuery] double lat, [FromQuery] double lng)
    {
        var district = await _service.GetDistrictByCoordsAsync(lat, lng);
        if (district == null) return NotFound("Территория не входит в зоны обслуживания.");
        return Ok(district);
    }
}