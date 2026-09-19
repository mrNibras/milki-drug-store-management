using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IDatabaseMonitoringService _dbMonitoringService;

    public HealthController(AppDbContext dbContext, IConfiguration configuration, IDatabaseMonitoringService dbMonitoringService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _dbMonitoringService = dbMonitoringService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            time = DateTime.UtcNow.ToString("o")
        });
    }

    [HttpGet("database")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Database()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        var provider = _dbContext.Database.ProviderName ?? "Unknown";
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";

        var sizeInfo = await _dbMonitoringService.GetDatabaseSizeInfoAsync();

        bool connectionSuccessful = false;
        string? connectionError = null;
        try
        {
            connectionSuccessful = await _dbContext.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            connectionError = ex.Message;
        }

        int pendingMigrations = 0;
        int appliedMigrations = 0;
        try
        {
            var pending = await _dbContext.Database.GetPendingMigrationsAsync();
            pendingMigrations = pending.Count();
            var applied = await _dbContext.Database.GetAppliedMigrationsAsync();
            appliedMigrations = applied.Count();
        }
        catch (Exception ex)
        {
            connectionError = ex.Message;
        }

        var isSqlite = provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase);
        var dataDirectory = isSqlite
            ? (_configuration.GetValue<string>("DataDirectory") ?? "/var/data")
            : null;
        var databasePath = isSqlite ? ExtractSqlitePath(connectionString) : null;
        var dbFile = isSqlite && databasePath != null ? new FileInfo(databasePath) : null;
        var (writable, writeError) = isSqlite ? CheckWritable(databasePath ?? "Unknown") : (false, (string?)null);

        return Ok(new
        {
            environment,
            databaseProvider = provider,
            connectionString = MaskConnectionString(connectionString),
            dataDirectory,
            databasePath,
            databaseExists = dbFile?.Exists ?? false,
            databaseWritable = writable,
            writeError,
            databaseSizeBytes = sizeInfo.SizeBytes,
            databaseSizeReadable = sizeInfo.SizeReadable,
            databaseSizeStatus = sizeInfo.Status,
            databaseSizeWarningThresholdBytes = sizeInfo.WarningThresholdBytes,
            databaseSizeCriticalThresholdBytes = sizeInfo.CriticalThresholdBytes,
            databaseLastModifiedUtc = dbFile?.Exists == true ? dbFile.LastWriteTimeUtc.ToString("o") : null,
            connectionSuccessful,
            connectionError,
            appliedMigrations,
            pendingMigrations
        });
    }

    private static string? ExtractSqlitePath(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return null;
        var prefix = "Data Source=";
        var idx = connectionString.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        var raw = connectionString.Substring(idx + prefix.Length).Trim();
        var end = raw.IndexOf(';');
        return end < 0 ? raw : raw.Substring(0, end);
    }

    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return string.Empty;

        if (connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("User Id=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("UserID=", StringComparison.OrdinalIgnoreCase))
        {
            var masked = new System.Text.StringBuilder(connectionString);
            foreach (var key in new[] { "Password=", "User Id=", "UserID=" })
            {
                var idx = masked.ToString().IndexOf(key, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) continue;
                var valueStart = idx + key.Length;
                var valueEnd = masked.ToString().IndexOf(';', valueStart);
                if (valueEnd < 0) valueEnd = masked.Length;
                masked.Remove(valueStart, valueEnd - valueStart);
                masked.Insert(valueStart, "***masked***");
            }
            return masked.ToString();
        }

        return connectionString;
    }

    private static (bool writable, string? error) CheckWritable(string databasePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(databasePath) || databasePath == "Unknown")
                return (false, "Path unknown");

            var dir = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                return (false, $"Directory does not exist: {dir}");

            var testPath = databasePath + ".write-test";
            System.IO.File.WriteAllText(testPath, string.Empty);
            System.IO.File.Delete(testPath);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
