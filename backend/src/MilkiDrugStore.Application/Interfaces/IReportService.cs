using MilkiDrugStore.Application.DTOs.Report;

namespace MilkiDrugStore.Application.Interfaces;

public interface IReportService
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(int? branchId = null);
    Task<IEnumerable<SalesReportResponse>> GetSalesReportAsync(string period, int? branchId = null);
    /// <summary>
    /// The business window a sales report covers, resolved in Ethiopian time and
    /// returned as UTC instants. Consumers use this instead of the browser clock
    /// so the detail table and the report chart share one period.
    /// </summary>
    Task<SalesPeriodBoundsResponse> GetSalesPeriodBoundsAsync(string period);
    Task<IEnumerable<InventoryReportResponse>> GetInventoryReportAsync(int? branchId = null);
    Task<IEnumerable<SupplierReportResponse>> GetSupplierReportAsync(int? branchId = null);
    Task<IEnumerable<StaffReportResponse>> GetStaffReportAsync(int? branchId = null);
}
