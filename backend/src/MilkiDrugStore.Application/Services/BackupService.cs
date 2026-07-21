using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Application.Services;

public class BackupService : IBackupService
{
    private readonly string _backupDirectory;
    private readonly string _connectionString;

    public BackupService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        _backupDirectory = configuration["BackupDirectory"] ?? "/var/data/backups";
        Directory.CreateDirectory(_backupDirectory);
    }

    public Task<string> CreateBackupAsync(string backupPath)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = Path.GetFileNameWithoutExtension(backupPath);
            var fullBackupPath = Path.Combine(_backupDirectory, $"{fileName}_{timestamp}.db");

            if (_connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("SQL Server backup is not yet implemented. Please use SQL Server Management Studio or Azure portal for backups.");
            }

            var dbPath = _connectionString.Replace("Data Source=", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("Database file not found.", dbPath);
            }

            File.Copy(dbPath, fullBackupPath, overwrite: true);
            return Task.FromResult(fullBackupPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Backup failed: {ex.Message}", ex);
        }
    }

    public Task<bool> RestoreBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Backup file not found.", backupPath);
            }

            if (_connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("SQL Server restore is not yet implemented. Please use SQL Server Management Studio or Azure portal for restores.");
            }

            var dbPath = _connectionString.Replace("Data Source=", "", StringComparison.OrdinalIgnoreCase).Trim();
            var tempPath = dbPath + ".restore_temp";

            File.Copy(backupPath, tempPath, overwrite: true);
            File.Replace(tempPath, dbPath, null);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Restore failed: {ex.Message}", ex);
        }
    }

    public Task<string> GetBackupDirectoryAsync()
    {
        return Task.FromResult(_backupDirectory);
    }
}
