using LocalEcho.Aplication.Interfaces;
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
    private readonly IUserService _userService;
    private readonly ICityService _cityService;
    

    public AdminController(IDistrictService districtService, IUserService userService, ICityService cityService)
    {
        _districtService = districtService;
        _userService = userService;
        _cityService = cityService;
    }
    
    [HttpGet("users/search")]
    public async Task<IActionResult> Search([FromQuery] string q)
        => Ok(await _userService.SearchUsersAsync(q));

    [HttpPost("users/{userId:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid userId, [FromBody] string role)
    {
        await _userService.AssignRoleAsync(userId, role);
        return Ok(new { success = true, message = $"Роль {role} успешно назначена." });
    }

    [HttpDelete("users/{userId:guid}/role/{role}")]
    public async Task<IActionResult> RemoveRole(Guid userId, string role)
    {
        await _userService.RemoveRoleAsync(userId, role);
        return Ok(new { success = true, message = "Роль отозвана." });
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
    [HttpPost("cities")]
    public async Task<IActionResult> CreateCity([FromBody] CreateCityDto dto)
    {
        var id = await _cityService.CreateAsync(dto);
        return CreatedAtAction(nameof(CreateCity), new { id }, dto);
    }

    [HttpPut("cities/{id:guid}")]
    public async Task<IActionResult> UpdateCity(Guid id, [FromBody] CreateCityDto dto)
    {
        await _cityService.UpdateAsync(id, dto);
        return NoContent();
    }
}
