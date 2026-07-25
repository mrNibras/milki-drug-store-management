using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(IAuthService authService, ILogger<SettingsController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var branchId = GetBranchId();
        var settings = await _authService.GetSettingsAsync(branchId > 0 ? branchId : null);
        return Ok(settings);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Unauthorized" });

        var branchId = GetBranchId();
        var result = await _authService.UpdateSettingsAsync(request, userId, branchId > 0 ? branchId : null);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("backup")]
    public async Task<IActionResult> BackupDatabase([FromServices] IBackupService backupService)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var fileName = $"milki-drug-store-backup-{timestamp}.bak";
            var backupPath = await backupService.CreateBackupAsync(fileName);

            if (!System.IO.File.Exists(backupPath))
                return NotFound(new { message = "Backup file was not created" });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(backupPath);
            var downloadName = Path.GetFileName(backupPath);
            return base.File(fileBytes, "application/octet-stream", downloadName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database backup failed");
            return BadRequest(new { message = $"Backup failed: {ex.Message}" });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("restore")]
    public async Task<IActionResult> RestoreDatabase([FromForm] IFormFile backupFile, [FromServices] IBackupService backupService)
    {
        if (backupFile == null || backupFile.Length == 0)
            return BadRequest(new { message = "No backup file uploaded" });

        var tempPath = Path.Combine(Path.GetTempPath(), $"restore_{Guid.NewGuid()}{Path.GetExtension(backupFile.FileName)}");

        try
        {
            await using (var stream = new System.IO.FileStream(tempPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
            {
                await backupFile.CopyToAsync(stream);
            }

            await backupService.RestoreBackupAsync(tempPath);

            return Ok(new { message = "Database restored successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database restore failed");
            return BadRequest(new { message = $"Restore failed: {ex.Message}" });
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
            {
                try { System.IO.File.Delete(tempPath); } catch { }
            }
        }
    }

    private int GetBranchId()
    {
        var branchIdClaim = User.FindFirst("branchId")?.Value;
        return int.TryParse(branchIdClaim, out var branchId) ? branchId : 0;
    }
}
