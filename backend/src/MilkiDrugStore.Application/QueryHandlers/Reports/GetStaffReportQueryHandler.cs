using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Reports;
using MilkiDrugStore.Application.DTOs.Report;

namespace MilkiDrugStore.Application.QueryHandlers.Reports;

public class GetStaffReportQueryHandler : IRequestHandler<GetStaffReportQuery, IEnumerable<StaffReportResponse>>
{
    private readonly IReportService _reportService;

    public GetStaffReportQueryHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IEnumerable<StaffReportResponse>> Handle(GetStaffReportQuery request, CancellationToken cancellationToken)
    {
        return await _reportService.GetStaffReportAsync(request.BranchId);
    }
}
