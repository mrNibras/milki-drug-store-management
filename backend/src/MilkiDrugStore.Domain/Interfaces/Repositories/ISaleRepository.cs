using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Domain.Interfaces.Repositories;

public interface ISaleRepository : IRepository<Sale>
{
    Task<Sale?> GetWithItemsAsync(int id);
    Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<int> GetNextSaleSequenceAsync(int year);
}
