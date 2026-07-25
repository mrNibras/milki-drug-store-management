using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MilkiDrugStore.Api.Models;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            time = DateTime.UtcNow.ToString("o")
        });
    }
}
