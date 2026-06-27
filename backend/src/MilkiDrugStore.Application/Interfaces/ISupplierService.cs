using MilkiDrugStore.Application.DTOs.Supplier;

namespace MilkiDrugStore.Application.Interfaces;

public interface ISupplierService
{
    Task<IEnumerable<SupplierResponse>> GetAllAsync();
    Task<SupplierResponse?> GetByIdAsync(int id);
    Task<SupplierResponse> CreateAsync(CreateSupplierRequest request, int userId);
    Task<SupplierResponse?> UpdateAsync(int id, UpdateSupplierRequest request, int userId);
    Task DeleteAsync(int id, int userId);
}
