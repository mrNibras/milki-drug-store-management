using MilkiDrugStore.Application.DTOs.Cosmetic;

namespace MilkiDrugStore.Application.Interfaces;

public interface ICosmeticService
{
    Task<IEnumerable<CosmeticResponse>> GetAllAsync(string? search = null, int? categoryId = null);
    Task<CosmeticResponse?> GetByIdAsync(int id);
    Task<CosmeticResponse> CreateAsync(CreateCosmeticRequest request, int userId);
    Task<CosmeticResponse?> UpdateAsync(int id, UpdateCosmeticRequest request, int userId);
    Task DeleteAsync(int id, int userId);
    Task<CosmeticResponse> AddBatchAsync(AddBatchRequest request, int userId);
}
