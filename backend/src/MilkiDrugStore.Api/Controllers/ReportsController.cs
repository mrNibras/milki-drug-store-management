using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;
using MediatR;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IMediator _mediator;

    public ReportsController(IReportService reportService, IMediator mediator)
    {
        _reportService = reportService;
        _mediator = mediator;
    }

    [HttpGet("dashboard/summary")]
    public async Task<IActionResult> GetDashboardSummary([FromQuery] int? branchId)
    {
        var result = await _mediator.Send(new Application.Queries.Reports.GetDashboardSummaryQuery(branchId));
        return Ok(result);
    }

    [HttpGet("sales/{period}")]
    public async Task<IActionResult> GetSalesReport(string period, [FromQuery] int? branchId)
    {
        var result = await _mediator.Send(new Application.Queries.Reports.GetSalesReportQuery(period, branchId));
        return Ok(result);
    }

    /// <summary>
    /// The business window <see cref="GetSalesReport"/> covers for a period, so a
    /// client can render detail rows from the same window instead of guessing
    /// from the browser clock. Available even when the period has no sales.
    /// </summary>
    [HttpGet("sales/{period}/bounds")]
    public async Task<IActionResult> GetSalesPeriodBounds(string period)
    {
        var bounds = await _reportService.GetSalesPeriodBoundsAsync(period);
        return Ok(bounds);
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport([FromQuery] int? branchId)
    {
        var result = await _mediator.Send(new Application.Queries.Reports.GetInventoryReportQuery(branchId));
        return Ok(result);
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSupplierReport([FromQuery] int? branchId)
    {
        var result = await _mediator.Send(new Application.Queries.Reports.GetSupplierReportQuery(branchId));
        return Ok(result);
    }

    [HttpGet("staff")]
    public async Task<IActionResult> GetStaffReport([FromQuery] int? branchId)
    {
        var result = await _mediator.Send(new Application.Queries.Reports.GetStaffReportQuery(branchId));
        return Ok(result);
    }
}
