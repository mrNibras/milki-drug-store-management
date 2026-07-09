using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public SettingsController(IAuthService authService, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _authService.GetSettingsAsync();
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Unauthorized" });

        var result = await _authService.UpdateSettingsAsync(request, userId);
        return Ok(result);
    }

    [HttpGet("backup")]
    public async Task<IActionResult> BackupDatabase()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            return BadRequest(new { message = "Database connection string not configured" });

        var dbPath = ExtractDatabasePath(connectionString);
        if (string.IsNullOrEmpty(dbPath) || !System.IO.File.Exists(dbPath))
            return NotFound(new { message = "Database file not found" });

        var fileBytes = await System.IO.File.ReadAllBytesAsync(dbPath);
        var fileName = $"milki-drug-store-backup-{DateTime.Now:yyyyMMdd-HHmmss}.db";
        return File(fileBytes, "application/octet-stream", fileName);
    }

    [HttpPost("restore")]
    public async Task<IActionResult> RestoreDatabase([FromForm] IFormFile backupFile)
    {
        if (backupFile == null || backupFile.Length == 0)
            return BadRequest(new { message = "No backup file uploaded" });

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            return BadRequest(new { message = "Database connection string not configured" });

        var dbPath = ExtractDatabasePath(connectionString);
        if (string.IsNullOrEmpty(dbPath))
            return BadRequest(new { message = "Invalid database path" });

        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.DisposeAsync();
            }

            await System.IO.File.WriteAllBytesAsync(dbPath, []);
            await using (var stream = new System.IO.FileStream(dbPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
            {
                await backupFile.CopyToAsync(stream);
            }

            return Ok(new { message = "Database restored successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Restore failed: {ex.Message }" });
        }
    }

    private static string? ExtractDatabasePath(string connectionString)
    {
        if (connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString["Data Source=".Length..].Trim();
        }
        return connectionString;
    }
}
