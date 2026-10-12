using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Domain.Interfaces;
using System.Data;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IDatabaseMonitoringService _dbMonitoringService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AppDbContext dbContext, IConfiguration configuration, IDatabaseMonitoringService dbMonitoringService, ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _dbMonitoringService = dbMonitoringService;
        _logger = logger;
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

    /// <summary>
    /// Diagnostic endpoint for debugging production issues.
    /// Returns detailed database schema and connectivity information.
    /// Requires Admin role.
    /// </summary>
    [HttpGet("diagnostics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Diagnostics()
    {
        var diagnostics = new Dictionary<string, object>();
        var errors = new List<string>();

        try
        {
            // Test basic connectivity
            var canConnect = await _dbContext.Database.CanConnectAsync();
            diagnostics["canConnect"] = canConnect;
        }
        catch (Exception ex)
        {
            diagnostics["canConnect"] = false;
            errors.Add($"Connectivity: {ex.Message}");
        }

        try
        {
            // Check applied migrations
            var applied = await _dbContext.Database.GetAppliedMigrationsAsync();
            diagnostics["appliedMigrations"] = applied.ToList();
            diagnostics["appliedMigrationsCount"] = applied.Count();
        }
        catch (Exception ex)
        {
            errors.Add($"AppliedMigrations: {ex.Message}");
        }

        try
        {
            // Check pending migrations
            var pending = await _dbContext.Database.GetPendingMigrationsAsync();
            diagnostics["pendingMigrations"] = pending.ToList();
            diagnostics["pendingMigrationsCount"] = pending.Count();
        }
        catch (Exception ex)
        {
            errors.Add($"PendingMigrations: {ex.Message}");
        }

        try
        {
            // Test Users table query (used by JWT validation)
            var userCount = await _dbContext.Users.CountAsync();
            diagnostics["usersTableAccessible"] = true;
            diagnostics["usersCount"] = userCount;
        }
        catch (Exception ex)
        {
            diagnostics["usersTableAccessible"] = false;
            errors.Add($"UsersTable: {ex.Message}");
        }

        try
        {
            // Test Suppliers table query (missing ContactPerson column)
            var supplierCount = await _dbContext.Suppliers.CountAsync();
            diagnostics["suppliersTableAccessible"] = true;
            diagnostics["suppliersCount"] = supplierCount;
        }
        catch (Exception ex)
        {
            diagnostics["suppliersTableAccessible"] = false;
            errors.Add($"SuppliersTable: {ex.Message}");
        }

        try
        {
            // Test Cosmetics table query
            var cosmeticCount = await _dbContext.Cosmetics.CountAsync();
            diagnostics["cosmeticsTableAccessible"] = true;
            diagnostics["cosmeticsCount"] = cosmeticCount;
        }
        catch (Exception ex)
        {
            diagnostics["cosmeticsTableAccessible"] = false;
            errors.Add($"CosmeticsTable: {ex.Message}");
        }

        try
        {
            // Test Medicines table query
            var medicineCount = await _dbContext.Medicines.CountAsync();
            diagnostics["medicinesTableAccessible"] = true;
            diagnostics["medicinesCount"] = medicineCount;
        }
        catch (Exception ex)
        {
            diagnostics["medicinesTableAccessible"] = false;
            errors.Add($"MedicinesTable: {ex.Message}");
        }

        try
        {
            // Test Purchases table query
            var purchaseCount = await _dbContext.Purchases.CountAsync();
            diagnostics["purchasesTableAccessible"] = true;
            diagnostics["purchasesCount"] = purchaseCount;
        }
        catch (Exception ex)
        {
            diagnostics["purchasesTableAccessible"] = false;
            errors.Add($"PurchasesTable: {ex.Message}");
        }

        // Database provider info
        diagnostics["databaseProvider"] = _dbContext.Database.ProviderName ?? "Unknown";

        // Connection string info (masked)
        var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        var databaseUrl = _configuration["DATABASE_URL"] ?? string.Empty;
        diagnostics["hasDefaultConnectionString"] = !string.IsNullOrEmpty(connectionString);
        diagnostics["hasDatabaseUrl"] = !string.IsNullOrEmpty(databaseUrl);
        diagnostics["effectiveConnectionString"] = DbProviderResolver.MaskConnectionString(
            DbProviderResolver.ResolveConnectionString(_configuration));

        // Environment info
        diagnostics["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
        diagnostics["timestamp"] = DateTime.UtcNow.ToString("o");

        if (errors.Count > 0)
        {
            diagnostics["errors"] = errors;
            return StatusCode(500, diagnostics);
        }

        return Ok(diagnostics);
    }

    /// <summary>
    /// Lightweight health check that doesn't require authentication.
    /// Tests basic database connectivity only.
    /// </summary>
    [HttpGet("ready")]
    [AllowAnonymous]
    public async Task<IActionResult> Ready()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            if (canConnect)
            {
                return Ok(new { status = "Ready", database = "Connected", time = DateTime.UtcNow.ToString("o") });
            }
            return StatusCode(503, new { status = "Not Ready", database = "Cannot Connect", time = DateTime.UtcNow.ToString("o") });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, new { status = "Not Ready", database = "Error", error = ex.Message, time = DateTime.UtcNow.ToString("o") });
        }
    }
}
