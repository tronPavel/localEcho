using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IDistrictService _districtService;

    public AdminController(IDistrictService districtService)
    {
        _districtService = districtService;
    }

    [HttpPost("districts")]
    public async Task<IActionResult> CreateDistrict([FromBody] CreateDistrictDto dto)
    {
        var id = await _districtService.CreateAsync(dto);
        return CreatedAtAction(nameof(CreateDistrict), new { id }, dto);
    }

    [HttpPut("districts/{id:guid}")]
    public async Task<IActionResult> UpdateDistrict(Guid id, [FromBody] CreateDistrictDto dto)
    {
        await _districtService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("districts/{id:guid}")]
    public async Task<IActionResult> DeleteDistrict(Guid id)
    {
        await _districtService.DeleteAsync(id);
        return NoContent();
    }
}
