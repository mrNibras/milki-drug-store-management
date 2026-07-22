using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? branchId)
    {
        var logs = await _auditLogService.GetAllAsync(branchId);
        return Ok(logs);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId, [FromQuery] int? branchId)
    {
        var logs = await _auditLogService.GetByUserAsync(userId, branchId);
        return Ok(logs);
    }

    [HttpGet("range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? branchId)
    {
        var logs = await _auditLogService.GetByDateRangeAsync(from, to, branchId);
        return Ok(logs);
    }
}
