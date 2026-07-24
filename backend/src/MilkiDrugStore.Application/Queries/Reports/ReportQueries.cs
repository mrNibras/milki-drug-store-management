using MediatR;
using MilkiDrugStore.Application.DTOs.Report;

namespace MilkiDrugStore.Application.Queries.Reports;

public record GetDashboardSummaryQuery(int? BranchId = null) : IRequest<DashboardSummaryResponse>;
public record GetSalesReportQuery(string Period, int? BranchId = null) : IRequest<IEnumerable<SalesReportResponse>>;
public record GetInventoryReportQuery(int? BranchId = null) : IRequest<IEnumerable<InventoryReportResponse>>;
public record GetSupplierReportQuery(int? BranchId = null) : IRequest<IEnumerable<SupplierReportResponse>>;
public record GetStaffReportQuery(int? BranchId = null) : IRequest<IEnumerable<StaffReportResponse>>;
