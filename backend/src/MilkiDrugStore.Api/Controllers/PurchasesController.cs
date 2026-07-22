using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchasesController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branchId = GetBranchId();
        var result = await _purchaseService.GetAllAsync(branchId > 0 ? branchId : null);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var branchId = GetBranchId();
        var result = await _purchaseService.GetByIdAsync(id, branchId > 0 ? branchId : null);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MilkiDrugStore.Application.DTOs.Purchase.CreatePurchaseRequest request)
    {
        var userId = GetUserId();
        var branchId = GetBranchId();
        var result = await _purchaseService.CreateAsync(request, userId, branchId > 0 ? branchId : null);
        return Ok(result);
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
