using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Catalog;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/lookups")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public LookupsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _catalogService.GetCategoryOptionsAsync();
        return Ok(categories);
    }

    [HttpGet("unit-types")]
    public async Task<IActionResult> GetUnitTypes()
    {
        var unitTypes = await _catalogService.GetUnitTypeOptionsAsync();
        return Ok(unitTypes);
    }
}