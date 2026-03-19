namespace LocalEcho.Application.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName);
    Task DeleteFileAsync(string fileUrl); 
}