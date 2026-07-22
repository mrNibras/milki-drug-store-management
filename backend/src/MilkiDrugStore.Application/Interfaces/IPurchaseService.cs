using MilkiDrugStore.Application.DTOs.Purchase;

namespace MilkiDrugStore.Application.Interfaces;

public interface IPurchaseService
{
    Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, int createdBy, int? branchId = null);
    Task<IEnumerable<PurchaseResponse>> GetAllAsync(int? branchId = null);
    Task<PurchaseResponse?> GetByIdAsync(int id, int? branchId = null);
}
