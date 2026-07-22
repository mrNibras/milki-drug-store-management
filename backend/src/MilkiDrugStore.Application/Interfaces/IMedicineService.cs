using MilkiDrugStore.Application.DTOs.Medicine;

namespace MilkiDrugStore.Application.Interfaces;

public interface IMedicineService
{
    Task<IEnumerable<MedicineResponse>> GetAllAsync(string? search = null, int? categoryId = null, int? branchId = null);
    Task<MedicineResponse?> GetByIdAsync(int id, int? branchId = null);
    Task<MedicineResponse> CreateAsync(CreateMedicineRequest request, int userId);
    Task<MedicineResponse?> UpdateAsync(int id, UpdateMedicineRequest request, int userId);
    Task DeleteAsync(int id, int userId);
    Task<MedicineResponse> AddBatchAsync(AddBatchRequest request, int userId, int? branchId = null);
}
