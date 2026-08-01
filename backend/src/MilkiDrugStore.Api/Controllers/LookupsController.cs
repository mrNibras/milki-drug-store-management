using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.DTOs.Catalog;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/lookups")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public LookupsController(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    /// <summary>
    /// Returns medicine/cosmetic categories: built-in categories first
    /// (in their fixed order, "Other" last), then user-defined custom
    /// categories sorted alphabetically.
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _catalog.GetCategoryOptionsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns unit types: built-in unit types first (in their fixed order,
    /// "Other" last), then user-defined custom unit types sorted alphabetically.
    /// </summary>
    [HttpGet("unit-types")]
    public async Task<IActionResult> GetUnitTypes()
    {
        var result = await _catalog.GetUnitTypeOptionsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Creates a custom category (the "Other" option). Rejects blank names,
    /// duplicates, and names that collide with a built-in category.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCatalogOptionRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _catalog.CreateCustomCategoryAsync(request.Name, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a custom unit type (the "Other" option). Rejects blank names,
    /// duplicates, and names that collide with a built-in unit type.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("unit-types")]
    public async Task<IActionResult> CreateUnitType([FromBody] CreateCatalogOptionRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _catalog.CreateCustomUnitTypeAsync(request.Name, userId);
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
