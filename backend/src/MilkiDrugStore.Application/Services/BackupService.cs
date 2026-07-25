using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MilkiDrugStore.Application.Services;

public class BackupService : IBackupService
{
    private readonly string _backupDirectory;
    private readonly string _connectionString;
    private readonly ILogger<BackupService> _logger;

    public BackupService(IConfiguration configuration, ILogger<BackupService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        _backupDirectory = configuration["BackupDirectory"] ?? "/var/data/backups";
        _logger = logger;
        Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<string> CreateBackupAsync(string fileName)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var safeName = string.IsNullOrWhiteSpace(fileName) ? "backup" : Path.GetFileNameWithoutExtension(fileName);
            var fullBackupPath = Path.Combine(_backupDirectory, $"{safeName}_{timestamp}.bak");

            if (_connectionString.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
            {
                return await BackupSqlServerAsync(fullBackupPath);
            }

            var dbPath = ExtractSqlitePath(_connectionString);
            if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
                throw new FileNotFoundException("Database file not found.", dbPath);

            var sqliteBackupPath = Path.Combine(_backupDirectory, $"{safeName}_{timestamp}.db");
            File.Copy(dbPath, sqliteBackupPath, overwrite: true);
            return sqliteBackupPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup failed");
            throw new InvalidOperationException($"Backup failed: {ex.Message}", ex);
        }
    }

    public async Task<bool> RestoreBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Backup file not found.", backupPath);

            if (_connectionString.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
            {
                return await RestoreSqlServerAsync(backupPath);
            }

            var dbPath = ExtractSqlitePath(_connectionString);
            if (string.IsNullOrEmpty(dbPath))
                throw new InvalidOperationException("Invalid database path");

            var tempPath = dbPath + ".restore_temp";
            File.Copy(backupPath, tempPath, overwrite: true);
            File.Replace(tempPath, dbPath, null);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Restore failed");
            throw new InvalidOperationException($"Restore failed: {ex.Message}", ex);
        }
    }

    public Task<string> GetBackupDirectoryAsync()
    {
        return Task.FromResult(_backupDirectory);
    }

    private async Task<string> BackupSqlServerAsync(string backupFilePath)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.InitialCatalog ?? builder.DataSource;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = $@"BACKUP DATABASE [{databaseName}] TO DISK = @backupPath WITH INIT, FORMAT, NAME = N'MilkiDrugStore Backup'";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@backupPath", backupFilePath);
        command.CommandTimeout = 300;
        await command.ExecuteNonQueryAsync();

        _logger.LogInformation("SQL Server backup completed: {Path}", backupFilePath);
        return backupFilePath;
    }

    private async Task<bool> RestoreSqlServerAsync(string backupFilePath)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.InitialCatalog ?? builder.DataSource;

        var masterConnectionString = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master",
            ConnectTimeout = 60
        }.ConnectionString;

        await using var masterConnection = new SqlConnection(masterConnectionString);
        await masterConnection.OpenAsync();

        await using var killCommand = new SqlCommand($@"
            IF EXISTS (SELECT name FROM sys.databases WHERE name = @dbName)
            BEGIN
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            END", masterConnection);
        killCommand.Parameters.AddWithValue("@dbName", databaseName);
        killCommand.CommandTimeout = 120;
        await killCommand.ExecuteNonQueryAsync();

        var sql = $@"RESTORE DATABASE [{databaseName}] FROM DISK = @backupPath WITH REPLACE, RECOVERY";
        await using var restoreCommand = new SqlCommand(sql, masterConnection);
        restoreCommand.Parameters.AddWithValue("@backupPath", backupFilePath);
        restoreCommand.CommandTimeout = 300;
        await restoreCommand.ExecuteNonQueryAsync();

        _logger.LogInformation("SQL Server restore completed from: {Path}", backupFilePath);
        return true;
    }

    private static string? ExtractSqlitePath(string connectionString)
    {
        if (connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString["Data Source=".Length..].Trim();
        }
        return connectionString;
    }
}
