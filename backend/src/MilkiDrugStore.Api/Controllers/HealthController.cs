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

        return Ok(new
        {
            environment,
            databaseProvider = provider,
            connectionString = DbProviderResolver.MaskConnectionString(connectionString),
            databaseSizeBytes = sizeInfo.SizeBytes,
            databaseSizeReadable = sizeInfo.SizeReadable,
            databaseSizeStatus = sizeInfo.Status,
            databaseSizeWarningThresholdBytes = sizeInfo.WarningThresholdBytes,
            databaseSizeCriticalThresholdBytes = sizeInfo.CriticalThresholdBytes,
            connectionSuccessful,
            connectionError,
            appliedMigrations,
            pendingMigrations
        });
    }
}
