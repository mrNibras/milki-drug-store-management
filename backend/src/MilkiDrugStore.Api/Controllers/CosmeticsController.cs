using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.DTOs.Cosmetic;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CosmeticsController : ControllerBase
{
    private readonly ICosmeticService _cosmeticService;

    public CosmeticsController(ICosmeticService cosmeticService)
    {
        _cosmeticService = cosmeticService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var result = await _cosmeticService.GetAllAsync(search, categoryId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _cosmeticService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCosmeticRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _cosmeticService.CreateAsync(request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCosmeticRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _cosmeticService.UpdateAsync(id, request, userId);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = GetUserId();
            await _cosmeticService.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("batches")]
    public async Task<IActionResult> AddBatch([FromBody] AddBatchRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _cosmeticService.AddBatchAsync(request, userId);
            return Ok(result);
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
}
