using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

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

    [HttpPost]
    public async Task<IActionResult> RecordExpired([FromBody] MilkiDrugStore.Application.DTOs.Sale.RecordExpiredRequest request)
    {
        var userId = GetUserId();
        await _inventoryService.RecordExpiredAsync(request.BatchId, request.Quantity, userId);
        return Ok(new { message = "Expired record added successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branchId = GetBranchId();
        var expired = await _inventoryService.GetExpiredAsync(branchId > 0 ? branchId : null);
        return Ok(expired);
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
