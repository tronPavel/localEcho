using Microsoft.AspNetCore.Hosting;
using  LocalEcho.Application.Interfaces;


namespace LocalEcho.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File stream is empty");

        // Путь к папке wwwroot/folderName
        var uploadPath = Path.Combine(_env.WebRootPath, folderName);
        
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        // Генерируем уникальное имя: GUID + оригинальное расширение
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        // Копируем поток в файл
        using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOutput);

        // Возвращаем относительный путь (например /uploads/abc.jpg)
        return $"/{folderName}/{uniqueFileName}";
    }
}