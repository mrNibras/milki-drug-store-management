using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.DTOs.Medicine;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicinesController : ControllerBase
{
    private readonly IMedicineService _medicineService;

    public MedicinesController(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var branchId = GetBranchId();
        var result = await _medicineService.GetAllAsync(search, categoryId, branchId > 0 ? branchId : null);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var branchId = GetBranchId();
        var result = await _medicineService.GetByIdAsync(id, branchId > 0 ? branchId : null);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMedicineRequest request)
    {
        var userId = GetUserId();
        var result = await _medicineService.CreateAsync(request, userId);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicineRequest request)
    {
        var userId = GetUserId();
        var result = await _medicineService.UpdateAsync(id, request, userId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        await _medicineService.DeleteAsync(id, userId);
        return NoContent();
    }

    [HttpPost("batches")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddBatch([FromBody] AddBatchRequest request)
    {
        var userId = GetUserId();
        var branchId = GetBranchId();
        var result = await _medicineService.AddBatchAsync(request, userId, branchId > 0 ? branchId : null);
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
