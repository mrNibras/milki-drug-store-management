using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard/summary")]
    public async Task<IActionResult> GetDashboardSummary([FromQuery] int? branchId)
    {
        var result = await _reportService.GetDashboardSummaryAsync(branchId);
        return Ok(result);
    }

    [HttpGet("sales/{period}")]
    public async Task<IActionResult> GetSalesReport(string period, [FromQuery] int? branchId)
    {
        var result = await _reportService.GetSalesReportAsync(period, branchId);
        return Ok(result);
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport([FromQuery] int? branchId)
    {
        var result = await _reportService.GetInventoryReportAsync(branchId);
        return Ok(result);
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSupplierReport([FromQuery] int? branchId)
    {
        var result = await _reportService.GetSupplierReportAsync(branchId);
        return Ok(result);
    }

    [HttpGet("staff")]
    public async Task<IActionResult> GetStaffReport([FromQuery] int? branchId)
    {
        var result = await _reportService.GetStaffReportAsync(branchId);
        return Ok(result);
    }
}
