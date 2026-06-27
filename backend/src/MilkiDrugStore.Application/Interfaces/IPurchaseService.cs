using MilkiDrugStore.Application.DTOs.Purchase;

namespace MilkiDrugStore.Application.Interfaces;

public interface IPurchaseService
{
    Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, int createdBy);
    Task<IEnumerable<PurchaseResponse>> GetAllAsync();
    Task<PurchaseResponse?> GetByIdAsync(int id);
}
