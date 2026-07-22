using MilkiDrugStore.Application.DTOs.Sale;

namespace MilkiDrugStore.Application.Interfaces;

public interface ISaleService
{
    Task<SaleResponse> CreateAsync(CreateSaleRequest request, int userId, string userRole, int? branchId = null);
    Task<IEnumerable<SaleResponse>> GetAllAsync(int? branchId = null);
    Task<SaleResponse?> GetByIdAsync(int id, int? branchId = null);
}
