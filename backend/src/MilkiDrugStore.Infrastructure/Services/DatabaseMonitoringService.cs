using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Infrastructure.Services;

public class DatabaseMonitoringService : IDatabaseMonitoringService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DatabaseMonitoringService> _logger;
    private readonly int _warningThresholdGB;
    private readonly int _criticalThresholdGB;

    public DatabaseMonitoringService(
        AppDbContext dbContext,
        IConfiguration configuration,
        ILogger<DatabaseMonitoringService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _warningThresholdGB = configuration.GetValue<int>("DatabaseMonitoring:WarningThresholdGB", 5);
        _criticalThresholdGB = configuration.GetValue<int>("DatabaseMonitoring:CriticalThresholdGB", 10);
    }

    public async Task<DatabaseSizeInfo> GetDatabaseSizeInfoAsync()
    {
        var warningBytes = (long)_warningThresholdGB * 1024 * 1024 * 1024;
        var criticalBytes = (long)_criticalThresholdGB * 1024 * 1024 * 1024;

        var sizeBytes = await GetPostgresSizeAsync();

        var status = sizeBytes >= criticalBytes ? "critical"
                    : sizeBytes >= warningBytes ? "warning"
                    : "ok";

        if (status != "ok")
        {
            _logger.LogWarning("Database size {SizeBytes} bytes exceeds {Status} threshold", sizeBytes, status);
        }

        return new DatabaseSizeInfo
        {
            SizeBytes = sizeBytes,
            SizeReadable = FormatBytes(sizeBytes),
            Status = status,
            WarningThresholdBytes = warningBytes,
            CriticalThresholdBytes = criticalBytes
        };
    }

    private async Task<long> GetPostgresSizeAsync()
    {
        try
        {
            var result = await _dbContext.Database.SqlQueryRaw<long>(
                "SELECT pg_database_size(current_database())").FirstOrDefaultAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query PostgreSQL database size");
            return 0;
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        var order = 0;
        var size = (double)bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
