using MilkiDrugStore.Application.DTOs.Report;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IReportService _reportService;

    public DashboardService(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(int? branchId = null)
    {
        return await _reportService.GetDashboardSummaryAsync(branchId);
    }
}
