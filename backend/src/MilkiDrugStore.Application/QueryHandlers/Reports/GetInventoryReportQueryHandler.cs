using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Reports;
using MilkiDrugStore.Application.DTOs.Report;

namespace MilkiDrugStore.Application.QueryHandlers.Reports;

public class GetInventoryReportQueryHandler : IRequestHandler<GetInventoryReportQuery, IEnumerable<InventoryReportResponse>>
{
    private readonly IReportService _reportService;

    public GetInventoryReportQueryHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IEnumerable<InventoryReportResponse>> Handle(GetInventoryReportQuery request, CancellationToken cancellationToken)
    {
        return await _reportService.GetInventoryReportAsync(request.BranchId);
    }
}
