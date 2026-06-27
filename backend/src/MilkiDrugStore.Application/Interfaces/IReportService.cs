using MilkiDrugStore.Application.DTOs.Report;

namespace MilkiDrugStore.Application.Interfaces;

public interface IReportService
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync();
    Task<IEnumerable<SalesReportResponse>> GetSalesReportAsync(string period);
    Task<IEnumerable<InventoryReportResponse>> GetInventoryReportAsync();
    Task<IEnumerable<SupplierReportResponse>> GetSupplierReportAsync();
    Task<IEnumerable<StaffReportResponse>> GetStaffReportAsync();
}
