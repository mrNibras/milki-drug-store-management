using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Domain.Interfaces.Repositories;

public interface IInventoryTransactionRepository : IRepository<InventoryTransaction>
{
    Task<IEnumerable<InventoryTransaction>> GetByMedicineAsync(int medicineId);
    Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(string transactionType);
    Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime from, DateTime to);
}
