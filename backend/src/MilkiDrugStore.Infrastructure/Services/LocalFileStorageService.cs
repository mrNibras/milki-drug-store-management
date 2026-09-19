using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _basePath = configuration.GetValue<string>("FileStorage:Local:BasePath") ?? "/var/data/uploads";
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream stream, string fileName, string? contentType = null)
    {
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName))
            safeName = $"{Guid.NewGuid()}.bin";

        var subfolder = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var dir = Path.Combine(_basePath, subfolder);
        Directory.CreateDirectory(dir);

        var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
        var fullPath = Path.Combine(dir, uniqueName);

        await using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fileStream);
        }

        _logger.LogInformation("File saved to {Path} ({ContentType})", fullPath, contentType ?? "unknown");
        return fullPath;
    }

    public async Task<Stream?> GetFileAsync(string path)
    {
        if (!File.Exists(path))
            return null;

        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public Task<bool> FileExistsAsync(string path)
    {
        return Task.FromResult(File.Exists(path));
    }

    public Task DeleteFileAsync(string path)
    {
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }
}
