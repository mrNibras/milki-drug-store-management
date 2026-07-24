using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Reports;
using MilkiDrugStore.Application.DTOs.Report;

namespace MilkiDrugStore.Application.QueryHandlers.Reports;

public class GetSalesReportQueryHandler : IRequestHandler<GetSalesReportQuery, IEnumerable<SalesReportResponse>>
{
    private readonly IReportService _reportService;

    public GetSalesReportQueryHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IEnumerable<SalesReportResponse>> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        return await _reportService.GetSalesReportAsync(request.Period, request.BranchId);
    }
}
