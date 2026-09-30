using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DamagesController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public DamagesController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost]
    public async Task<IActionResult> RecordDamage([FromBody] MilkiDrugStore.Application.DTOs.Sale.RecordDamageRequest request)
    {
        var userId = GetUserId();
        await _inventoryService.RecordDamageAsync(request.BatchId, request.CosmeticBatchId, request.Quantity, request.Reason, userId);
        return Ok(new { message = "Damage recorded successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branchId = GetBranchId();
        var damages = await _inventoryService.GetDamagesAsync(branchId > 0 ? branchId : null);
        return Ok(damages);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private int GetBranchId()
    {
        var branchIdClaim = User.FindFirst("branchId")?.Value;
        return int.TryParse(branchIdClaim, out var branchId) ? branchId : 0;
    }
}
