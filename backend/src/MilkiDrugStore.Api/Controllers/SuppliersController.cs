using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Services;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly SupplierService _supplierService;

    public SuppliersController(SupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _supplierService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _supplierService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MilkiDrugStore.Application.DTOs.Supplier.CreateSupplierRequest request)
    {
        var userId = GetUserId();
        var result = await _supplierService.CreateAsync(request, userId);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MilkiDrugStore.Application.DTOs.Supplier.UpdateSupplierRequest request)
    {
        var userId = GetUserId();
        var result = await _supplierService.UpdateAsync(id, request, userId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        await _supplierService.DeleteAsync(id, userId);
        return NoContent();
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
