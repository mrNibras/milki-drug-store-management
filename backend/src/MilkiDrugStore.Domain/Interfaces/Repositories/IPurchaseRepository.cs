using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Domain.Interfaces.Repositories;

public interface IPurchaseRepository : IRepository<Purchase>
{
    Task<Purchase?> GetWithItemsAsync(int id);
    Task<int> GetNextPurchaseSequenceAsync(int year);
}
