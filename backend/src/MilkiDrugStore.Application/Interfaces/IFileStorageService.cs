namespace MilkiDrugStore.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream stream, string fileName, string? contentType = null);
    Task<Stream?> GetFileAsync(string path);
    Task<bool> FileExistsAsync(string path);
    Task DeleteFileAsync(string path);
}
