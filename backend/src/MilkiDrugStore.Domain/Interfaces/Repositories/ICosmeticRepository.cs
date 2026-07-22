using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Domain.Interfaces.Repositories;

public interface ICosmeticRepository : IRepository<Cosmetic>
{
    Task<IEnumerable<Cosmetic>> SearchAsync(string searchTerm);
    Task<IEnumerable<Cosmetic>> GetLowStockAsync();
    Task<IEnumerable<Cosmetic>> GetExpiringAsync(int months);
    Task<IEnumerable<Cosmetic>> GetOutOfStockAsync();
}
