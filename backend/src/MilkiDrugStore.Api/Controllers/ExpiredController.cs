using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Services;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpiredController : ControllerBase
{
    private readonly InventoryService _inventoryService;

    public ExpiredController(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost]
    public async Task<IActionResult> RecordExpired([FromBody] MilkiDrugStore.Application.DTOs.Sale.RecordExpiredRequest request)
    {
        var userId = GetUserId();
        await _inventoryService.RecordExpiredAsync(request.BatchId, request.Quantity, userId);
        return Ok(new { message = "Expired record added successfully" });
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
