using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Reports;
using MilkiDrugStore.Application.DTOs.Report;

namespace MilkiDrugStore.Application.QueryHandlers.Reports;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    private readonly IReportService _reportService;

    public GetDashboardSummaryQueryHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<DashboardSummaryResponse> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _reportService.GetDashboardSummaryAsync(request.BranchId);
    }
}
