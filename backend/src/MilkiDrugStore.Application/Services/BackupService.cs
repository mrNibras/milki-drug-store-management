using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Persistence.Context;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace MilkiDrugStore.Application.Services;

public class BackupService : IBackupService
{
    private readonly string _backupDirectory;
    private readonly string _connectionString;
    private readonly string _dbProvider;
    private readonly ILogger<BackupService> _logger;

    public BackupService(IConfiguration configuration, ILogger<BackupService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        _dbProvider = DbProviderResolver.DetectProvider(configuration);
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

            if (_dbProvider == DbProviderResolver.PostgreSql)
                return await BackupPostgresAsync(safeName, timestamp, _backupDirectory);

            if (_connectionString.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
                return await BackupSqlServerAsync(Path.Combine(_backupDirectory, $"{safeName}_{timestamp}.bak"));

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

            if (_dbProvider == DbProviderResolver.PostgreSql)
                return await RestorePostgresAsync(backupPath);

            if (_connectionString.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
                return await RestoreSqlServerAsync(backupPath);

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
