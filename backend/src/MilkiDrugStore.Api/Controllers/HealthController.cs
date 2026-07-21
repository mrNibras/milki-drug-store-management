using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Api.Models;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("health")]
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
