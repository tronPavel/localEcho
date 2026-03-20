using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalEcho.API.Controllers;

public record DeleteFileDto(string Url);

[ApiController][Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded");
        using var stream = file.OpenReadStream();
        var url = await _fileService.SaveFileAsync(stream, file.FileName, "uploads");
        return Ok(new { url });
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFile([FromBody] DeleteFileDto dto)
    {
        if (string.IsNullOrEmpty(dto.Url)) return BadRequest();
        
        if (!dto.Url.StartsWith("/uploads/")) return Forbid("Нельзя удалять файлы из этой директории.");

        await _fileService.DeleteFileAsync(dto.Url);
        return NoContent();
    }
}