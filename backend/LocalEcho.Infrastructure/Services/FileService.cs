using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace LocalEcho.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string folderName)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File stream is empty");

        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        
        var uploadPath = Path.Combine(webRootPath, folderName);
        

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
        var fullFilePath = Path.Combine(uploadPath, uniqueFileName);

        await using var outputStream = new FileStream(fullFilePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream);

        var publicUrl = $"/{folderName}/{uniqueFileName}";


        return publicUrl;
    }
}