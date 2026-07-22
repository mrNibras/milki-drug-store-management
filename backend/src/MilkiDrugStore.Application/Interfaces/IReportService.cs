using MilkiDrugStore.Application.DTOs.Report;

namespace MilkiDrugStore.Application.Interfaces;

public interface IReportService
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(int? branchId = null);
    Task<IEnumerable<SalesReportResponse>> GetSalesReportAsync(string period, int? branchId = null);
    Task<IEnumerable<InventoryReportResponse>> GetInventoryReportAsync(int? branchId = null);
    Task<IEnumerable<SupplierReportResponse>> GetSupplierReportAsync(int? branchId = null);
    Task<IEnumerable<StaffReportResponse>> GetStaffReportAsync(int? branchId = null);
}
