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
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        await _userService.UpdateProfileAsync(GetCurrentUserId(), dto);
        return Ok(new { success = true, message = "Profile updated" });
    }

    [HttpPost("change-district")]
    public async Task<IActionResult> ChangeDistrict([FromBody] ChangeDistrictDto dto)
    {
        await _userService.ChangeDistrictAsync(GetCurrentUserId(), dto);
        return Ok(new { success = true, message = "District changed" });
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded");

        try
        {
            var profile = await _userService.GetProfileAsync(GetCurrentUserId());
            var oldAvatarUrl = profile.AvatarUrl;

            using var stream = file.OpenReadStream();
            var newAvatarUrl = await _fileService.SaveFileAsync(stream, file.FileName, "avatars");

            if (!string.IsNullOrEmpty(oldAvatarUrl))
            {
                await _fileService.DeleteFileAsync(oldAvatarUrl);
            }

            await _userService.UpdateAvatarAsync(GetCurrentUserId(), newAvatarUrl);
            return Ok(new { success = true, avatarUrl = newAvatarUrl });
        }
        catch (ArgumentException ex) 
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Внутренняя ошибка сервера" });
        }
    }
}