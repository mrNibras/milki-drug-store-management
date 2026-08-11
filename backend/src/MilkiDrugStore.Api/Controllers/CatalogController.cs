using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.DTOs.Catalog;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public CatalogController(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    /// <summary>
    /// Returns medicine/cosmetic categories: built-in (system) categories first
    /// (in their fixed order, "Other" last), then user-defined custom categories
    /// sorted alphabetically.
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _catalog.GetCategoryOptionsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns unit types: built-in (system) unit types first (in their fixed order,
    /// "Other" last), then user-defined custom unit types sorted alphabetically.
    /// </summary>
    [HttpGet("unit-types")]
    public async Task<IActionResult> GetUnitTypes()
    {
        var result = await _catalog.GetUnitTypeOptionsAsync();
        return Ok(result);
    }
}
