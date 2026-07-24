using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Reports;
using MilkiDrugStore.Application.DTOs.Report;

namespace MilkiDrugStore.Application.QueryHandlers.Reports;

public class GetSupplierReportQueryHandler : IRequestHandler<GetSupplierReportQuery, IEnumerable<SupplierReportResponse>>
{
    private readonly IReportService _reportService;

    public GetSupplierReportQueryHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IEnumerable<SupplierReportResponse>> Handle(GetSupplierReportQuery request, CancellationToken cancellationToken)
    {
        return await _reportService.GetSupplierReportAsync(request.BranchId);
    }
}
