using MilkiDrugStore.Application.DTOs.Branch;

namespace MilkiDrugStore.Application.Interfaces;

public interface IBranchService
{
    Task<IEnumerable<BranchResponse>> GetAllAsync();
    Task<BranchResponse?> GetByIdAsync(int id);
    Task<BranchResponse> CreateAsync(CreateBranchRequest request, int createdBy);
    Task<BranchResponse?> UpdateAsync(int id, UpdateBranchRequest request, int updatedBy);
    Task DeleteAsync(int id, int deletedBy);
}
