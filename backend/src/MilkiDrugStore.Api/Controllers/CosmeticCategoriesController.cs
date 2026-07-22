using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.DTOs.Cosmetic;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CosmeticCategoriesController : ControllerBase
{
    private readonly ICosmeticCategoryService _cosmeticCategoryService;

    public CosmeticCategoriesController(ICosmeticCategoryService cosmeticCategoryService)
    {
        _cosmeticCategoryService = cosmeticCategoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _cosmeticCategoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _cosmeticCategoryService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCosmeticCategoryRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _cosmeticCategoryService.CreateAsync(request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCosmeticCategoryRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _cosmeticCategoryService.UpdateAsync(id, request, userId);
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
            await _cosmeticCategoryService.DeleteAsync(id, userId);
            return NoContent();
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
