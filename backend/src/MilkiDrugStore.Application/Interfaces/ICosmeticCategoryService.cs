using MilkiDrugStore.Application.DTOs.Cosmetic;

namespace MilkiDrugStore.Application.Interfaces;

public interface ICosmeticCategoryService
{
    Task<IEnumerable<CosmeticCategoryResponse>> GetAllAsync();
    Task<CosmeticCategoryResponse?> GetByIdAsync(int id);
    Task<CosmeticCategoryResponse> CreateAsync(CreateCosmeticCategoryRequest request, int userId);
    Task<CosmeticCategoryResponse?> UpdateAsync(int id, UpdateCosmeticCategoryRequest request, int userId);
    Task DeleteAsync(int id, int userId);
}
