using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFileService _fileService;

    public UsersController(IUserService userService, IFileService fileService)
    {
        _userService = userService;
        _fileService = fileService;
    }

    private Guid GetCurrentUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(idStr) ? Guid.Empty : Guid.Parse(idStr);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _userService.GetProfileAsync(GetCurrentUserId());
        return Ok(new { success = true, data = profile });
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto) // [FromForm]!
    {
        var userId = GetCurrentUserId();
        await _userService.UpdateProfileAsync(userId, dto);
        return Ok(new { success = true, message = "Profile updated" });
    }
}