using LocalEcho.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace LocalEcho.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    // магические байты для форматов изображений, чтоб не закинули .exe файл под видом другого
    private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new()
    {
        { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpg",  new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".png",  new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".webp", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46 } } } 
    };

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string folderName)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("Файл пуст.");

        if (fileStream.Length > 5 * 1024 * 1024)
            throw new ArgumentException("Размер файла не должен превышать 5 МБ.");

        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!_fileSignatures.ContainsKey(ext))
            throw new ArgumentException("Неподдерживаемый тип файла. Допустимы только JPG, PNG и WEBP.");

        using (var reader = new BinaryReader(fileStream, System.Text.Encoding.UTF8, true))
        {
            var maxSignatureLength = _fileSignatures[ext].Max(m => m.Length);
            var headerBytes = reader.ReadBytes(maxSignatureLength);

            var isMatch = _fileSignatures[ext].Any(signature => 
                headerBytes.Take(signature.Length).SequenceEqual(signature));

            if (!isMatch)
                throw new ArgumentException("Поддельный файл: содержимое не соответствует расширению.");
            
            fileStream.Position = 0; 
        }

        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadPath = Path.Combine(webRootPath, folderName);
        
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
        var fullFilePath = Path.Combine(uploadPath, uniqueFileName);

        await using var outputStream = new FileStream(fullFilePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream);

        return $"/{folderName}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;

        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var relativePath = fileUrl.TrimStart('/'); 
        var fullPath = Path.Combine(webRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}