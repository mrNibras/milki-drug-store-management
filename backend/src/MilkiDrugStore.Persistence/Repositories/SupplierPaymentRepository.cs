using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class SupplierPaymentRepository : Repository<SupplierPayment>, ISupplierPaymentRepository
{
    public SupplierPaymentRepository(AppDbContext context) : base(context) { }

    public async Task<IQueryable<SupplierPayment>> GetBySupplierAsync(int supplierId, int? branchId = null)
    {
        var query = _dbSet
            .Include(sp => sp.Items)
                .ThenInclude(ip => ip.Purchase)
            .Where(sp => sp.SupplierId == supplierId)
            .AsQueryable();

        if (branchId.HasValue)
            query = query.Where(sp => sp.BranchId == branchId.Value);

        return await Task.FromResult(query);
    }

    public async Task<SupplierPayment?> GetWithItemsAsync(int supplierPaymentId)
    {
        return await _dbSet
            .Include(sp => sp.Items)
                .ThenInclude(ip => ip.Purchase)
            .FirstOrDefaultAsync(sp => sp.SupplierPaymentId == supplierPaymentId);
    }

    public async Task<SupplierPaymentItem> AddPaymentItemAsync(SupplierPaymentItem item)
    {
        await _context.Set<SupplierPaymentItem>().AddAsync(item);
        return item;
    }
}