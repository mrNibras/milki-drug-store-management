using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.DTOs.Medicine;
using MilkiDrugStore.Application.Services;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicinesController : ControllerBase
{
    private readonly MedicineService _medicineService;

    public MedicinesController(MedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var result = await _medicineService.GetAllAsync(search, categoryId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _medicineService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMedicineRequest request)
    {
        var result = await _medicineService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicineRequest request)
    {
        var result = await _medicineService.UpdateAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _medicineService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("batches")]
    public async Task<IActionResult> AddBatch([FromBody] AddBatchRequest request)
    {
        var result = await _medicineService.AddBatchAsync(request);
        return Ok(result);
    }
}
