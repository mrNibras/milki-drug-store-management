using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

/// <summary>
/// Read-only access to expiry history. Expiry is no longer recorded manually:
/// <c>IInventoryService.ProcessExpiredInventoryAsync</c> writes off expired
/// medicine and cosmetic batches automatically, so this controller intentionally
/// exposes no create/update endpoint.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpiredController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public ExpiredController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branchId = GetBranchId();
        var expired = await _inventoryService.GetExpiredAsync(branchId > 0 ? branchId : null);
        return Ok(expired);
    }

    private int GetBranchId()
    {
        var branchIdClaim = User.FindFirst("branchId")?.Value;
        return int.TryParse(branchIdClaim, out var branchId) ? branchId : 0;
    }
}
