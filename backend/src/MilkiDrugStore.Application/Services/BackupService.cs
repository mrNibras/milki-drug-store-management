using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Persistence.Context;
using System.Diagnostics;

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

            return await BackupPostgresAsync(safeName, timestamp, _backupDirectory);
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

            return await RestorePostgresAsync(backupPath);
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

    private async Task<string> BackupPostgresAsync(string safeName, string timestamp, string backupDir)
    {
        var backupPath = Path.Combine(backupDir, $"{safeName}_{timestamp}.dump");
        var connInfo = NormalizePostgresConnectionString(_connectionString);

        var psi = new ProcessStartInfo
        {
            FileName = "pg_dump",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        psi.ArgumentList.Add("--host");
        psi.ArgumentList.Add(connInfo.Host);
        psi.ArgumentList.Add("--port");
        psi.ArgumentList.Add(connInfo.Port);
        psi.ArgumentList.Add("--username");
        psi.ArgumentList.Add(connInfo.Username);
        psi.ArgumentList.Add("--dbname");
        psi.ArgumentList.Add(connInfo.Database);
        psi.ArgumentList.Add("--file");
        psi.ArgumentList.Add(backupPath);
        psi.ArgumentList.Add("--format");
        psi.ArgumentList.Add("custom");
        psi.ArgumentList.Add("--no-password");
        psi.ArgumentList.Add("--verbose");

        psi.EnvironmentVariables["PGPASSWORD"] = connInfo.Password ?? string.Empty;

        _logger.LogInformation("Starting PostgreSQL backup to {Path} using pg_dump", backupPath);

        using var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start pg_dump process");

        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"pg_dump failed with exit code {process.ExitCode}: {stderr}");

        _logger.LogInformation("PostgreSQL backup completed: {Path}", backupPath);
        return backupPath;
    }

    private async Task<bool> RestorePostgresAsync(string backupPath)
    {
        var connInfo = NormalizePostgresConnectionString(_connectionString);

        var psi = new ProcessStartInfo
        {
            FileName = "pg_restore",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        psi.ArgumentList.Add("--host");
        psi.ArgumentList.Add(connInfo.Host);
        psi.ArgumentList.Add("--port");
        psi.ArgumentList.Add(connInfo.Port);
        psi.ArgumentList.Add("--username");
        psi.ArgumentList.Add(connInfo.Username);
        psi.ArgumentList.Add("--dbname");
        psi.ArgumentList.Add(connInfo.Database);
        psi.ArgumentList.Add("--clean");
        psi.ArgumentList.Add("--if-exists");
        psi.ArgumentList.Add("--verbose");
        psi.ArgumentList.Add(backupPath);

        psi.EnvironmentVariables["PGPASSWORD"] = connInfo.Password ?? string.Empty;

        _logger.LogInformation("Starting PostgreSQL restore from {Path}", backupPath);

        using var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start pg_restore process");

        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"pg_restore failed with exit code {process.ExitCode}: {stderr}");

        _logger.LogInformation("PostgreSQL restore completed from: {Path}", backupPath);
        return true;
    }

    private static PostgresConnectionInfo NormalizePostgresConnectionString(string connectionString)
    {
        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = DbProviderResolver.PostgresUrlToConnectionString(connectionString);
        }

        return ParsePostgresKV(connectionString);
    }

    private static PostgresConnectionInfo ParsePostgresKV(string connectionString)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = part.IndexOf('=');
            if (idx > 0)
            {
                var key = part.Substring(0, idx).Trim();
                var value = part.Substring(idx + 1).Trim();
                if (!string.IsNullOrEmpty(key))
                    dict[key] = value;
            }
        }

        return new PostgresConnectionInfo
        {
            Host = dict.GetValueOrDefault("Host") ?? dict.GetValueOrDefault("Server") ?? "localhost",
            Port = dict.GetValueOrDefault("Port") ?? "5432",
            Database = dict.GetValueOrDefault("Database") ?? dict.GetValueOrDefault("DB") ?? "milki_drug_store",
            Username = dict.GetValueOrDefault("Username") ?? dict.GetValueOrDefault("User Id") ?? "postgres",
            Password = dict.GetValueOrDefault("Password") ?? string.Empty
        };
    }

    private class PostgresConnectionInfo
    {
        public string Host { get; set; } = "localhost";
        public string Port { get; set; } = "5432";
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
