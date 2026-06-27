using MilkiDrugStore.Application.DTOs.Medicine;

namespace MilkiDrugStore.Application.Interfaces;

public interface IMedicineService
{
    Task<IEnumerable<MedicineResponse>> GetAllAsync(string? search = null, int? categoryId = null);
    Task<MedicineResponse?> GetByIdAsync(int id);
    Task<MedicineResponse> CreateAsync(CreateMedicineRequest request);
    Task<MedicineResponse?> UpdateAsync(int id, UpdateMedicineRequest request);
    Task DeleteAsync(int id);
    Task<MedicineResponse> AddBatchAsync(AddBatchRequest request);
}
