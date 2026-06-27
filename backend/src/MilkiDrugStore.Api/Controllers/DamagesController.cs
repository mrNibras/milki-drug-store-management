using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Services;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DamagesController : ControllerBase
{
    private readonly InventoryService _inventoryService;

    public DamagesController(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost]
    public async Task<IActionResult> RecordDamage([FromBody] MilkiDrugStore.Application.DTOs.Sale.RecordDamageRequest request)
    {
        var userId = GetUserId();
        await _inventoryService.RecordDamageAsync(request.BatchId, request.Quantity, request.Reason, userId);
        return Ok(new { message = "Damage recorded successfully" });
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
