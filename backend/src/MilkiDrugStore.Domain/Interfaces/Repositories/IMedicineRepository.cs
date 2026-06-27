using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Domain.Interfaces.Repositories;

public interface IMedicineRepository : IRepository<Medicine>
{
    Task<IEnumerable<Medicine>> SearchAsync(string searchTerm);
    Task<IEnumerable<Medicine>> GetLowStockAsync();
    Task<IEnumerable<Medicine>> GetExpiringAsync(int months);
    Task<IEnumerable<Medicine>> GetOutOfStockAsync();
}
