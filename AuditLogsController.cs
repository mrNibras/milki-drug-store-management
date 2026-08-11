using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly IClaimsProvider _claimsProvider;

    public AuditLogsController(IAuditLogService auditLogService, IClaimsProvider claimsProvider)
    {
        _auditLogService = auditLogService;
        _claimsProvider = claimsProvider;
    }

    /// <summary>
    /// Gets all audit logs for the current user's branch.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var branchId = _claimsProvider.GetBranchId();
        var logs = await _auditLogService.GetAllAsync(branchId);
        return Ok(logs);
    }
}