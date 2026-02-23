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
        
        // Валидация файла (можно вынести в FileService)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext)) return BadRequest("Invalid file type");

        try
        {
            using var stream = file.OpenReadStream();
            var avatarUrl = await _fileService.SaveFileAsync(stream, file.FileName, "avatars");
            await _userService.UpdateAvatarAsync(GetCurrentUserId(), avatarUrl);
            return Ok(new { success = true, avatarUrl = avatarUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}