using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branchId = GetBranchId();
        var result = await _saleService.GetAllAsync(branchId > 0 ? branchId : null);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var branchId = GetBranchId();
        var result = await _saleService.GetByIdAsync(id, branchId > 0 ? branchId : null);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MilkiDrugStore.Application.DTOs.Sale.CreateSaleRequest request)
    {
        var userId = GetUserId();
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Pharmacist";
        var branchId = GetBranchId();
        try
        {
            var result = await _saleService.CreateAsync(request, userId, userRole, branchId > 0 ? branchId : null);
            return Ok(result);
        }
        catch (MilkiDrugStore.Domain.Exceptions.InsufficientStockException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
