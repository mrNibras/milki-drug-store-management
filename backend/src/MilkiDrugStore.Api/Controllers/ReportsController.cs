using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Services;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard/summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var result = await _reportService.GetDashboardSummaryAsync();
        return Ok(result);
    }

    [HttpGet("sales/{period}")]
    public async Task<IActionResult> GetSalesReport(string period)
    {
        var result = await _reportService.GetSalesReportAsync(period);
        return Ok(result);
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport()
    {
        var result = await _reportService.GetInventoryReportAsync();
        return Ok(result);
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSupplierReport()
    {
        var result = await _reportService.GetSupplierReportAsync();
        return Ok(result);
    }

    [HttpGet("staff")]
    public async Task<IActionResult> GetStaffReport()
    {
        var result = await _reportService.GetStaffReportAsync();
        return Ok(result);
    }
}
