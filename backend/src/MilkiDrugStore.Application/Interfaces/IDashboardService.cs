using MilkiDrugStore.Application.DTOs.Report;

namespace MilkiDrugStore.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync();
}
