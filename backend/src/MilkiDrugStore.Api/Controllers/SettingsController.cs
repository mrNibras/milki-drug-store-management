using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IAuthService _authService;

    public SettingsController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _authService.GetSettingsAsync();
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        var result = await _authService.UpdateSettingsAsync(request);
        return Ok(result);
    }
}
