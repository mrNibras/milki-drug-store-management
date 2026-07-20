using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;
using System.Linq;

namespace MilkiDrugStore.Persistence.Repositories;

public class SaleRepository : Repository<Sale>, ISaleRepository
{
    public SaleRepository(AppDbContext context) : base(context) { }

    public override async Task<IQueryable<Sale>> GetAllAsync()
    {
        return await Task.FromResult(_dbSet
            .Include(s => s.Items)
                .ThenInclude(i => i.Medicine)
            .Include(s => s.Items)
                .ThenInclude(i => i.Batch)
            .Include(s => s.User)
            .AsQueryable());
    }

    public async Task<Sale?> GetWithItemsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Items)
            .ThenInclude(i => i.Medicine)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SaleId == id);
    }

    public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(s => s.SaleDate >= from && s.SaleDate <= to)
            .Include(s => s.User)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }
}
