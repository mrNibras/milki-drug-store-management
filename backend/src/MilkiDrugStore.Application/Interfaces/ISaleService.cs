using MilkiDrugStore.Application.DTOs.Sale;

namespace MilkiDrugStore.Application.Interfaces;

public interface ISaleService
{
    Task<SaleResponse> CreateAsync(CreateSaleRequest request, int userId);
    Task<IEnumerable<SaleResponse>> GetAllAsync();
    Task<SaleResponse?> GetByIdAsync(int id);
}
