using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LocalEcho.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(new { success = true, message = "Registered", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(new { success = true, message = "Logged in", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return Unauthorized(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(dto);
            return Ok(new { success = true, message = "Token refreshed", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh error");
            return Unauthorized(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = Guid.Parse(userIdStr);
        await _authService.LogoutAsync(userId);
        return Ok(new { success = true, message = "Logged out" });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var profile = await _authService.GetProfileAsync(userId);
            return Ok(new { success = true, data = profile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Profile error");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var success = await _authService.UpdateProfileAsync(userId, dto);
            return success ? Ok(new { success = true, message = "Profile updated" }) : BadRequest(new { success = false, error = "Update failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update profile error");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("change-district")]
    [Authorize]
    public async Task<IActionResult> ChangeDistrict([FromBody] ChangeDistrictDto dto)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var success = await _authService.ChangeDistrictAsync(userId, dto);
            return success ? Ok(new { success = true, message = "District changed" }) : BadRequest(new { success = false, error = "Change failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Change district error");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("districts")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDistricts()
    {
        try
        {
            var districts = await _authService.GetAllDistrictsAsync();
            return Ok(new { success = true, data = districts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get districts error");
            return StatusCode(500, new { success = false, error = "Server error" });
        }
    }
}