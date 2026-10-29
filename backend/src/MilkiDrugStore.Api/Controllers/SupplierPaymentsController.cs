using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.DTOs.SupplierPayment;
using MilkiDrugStore.Application.Interfaces;
using System.Security.Claims;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Pharmacist")]
public class SupplierPaymentsController : ControllerBase
{
    private readonly ISupplierPaymentService _supplierPaymentService;

    public SupplierPaymentsController(ISupplierPaymentService supplierPaymentService)
    {
        _supplierPaymentService = supplierPaymentService;
    }

    [HttpGet("debts")]
    public async Task<IActionResult> GetOutstandingDebts([FromQuery] int? branchId)
    {
        var result = await _supplierPaymentService.GetOutstandingDebtsAsync(branchId);
        return Ok(result);
    }

    [HttpGet("debts/{supplierId}")]
    public async Task<IActionResult> GetSupplierDebt(int supplierId, [FromQuery] int? branchId)
    {
        var result = await _supplierPaymentService.GetSupplierDebtAsync(supplierId, branchId);
        if (result == null) return Ok(new { outstandingDebt = 0, outstandingPurchases = new List<object>() });
        return Ok(result);
    }

    [HttpGet("outstanding/{supplierId}")]
    public async Task<IActionResult> GetOutstandingPurchases(int supplierId, [FromQuery] int? branchId)
    {
        var result = await _supplierPaymentService.GetOutstandingPurchasesAsync(supplierId, branchId);
        return Ok(result);
    }

    [HttpGet("history/{supplierId}")]
    public async Task<IActionResult> GetPaymentHistory(int supplierId, [FromQuery] int? branchId)
    {
        var result = await _supplierPaymentService.GetPaymentHistoryAsync(supplierId, branchId);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RecordPayment([FromBody] CreateSupplierPaymentRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        try
        {
            var result = await _supplierPaymentService.RecordPaymentAsync(request, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
