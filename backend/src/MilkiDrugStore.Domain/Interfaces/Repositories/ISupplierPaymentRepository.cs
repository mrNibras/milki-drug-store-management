using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Domain.Interfaces.Repositories;

public interface ISupplierPaymentRepository : IRepository<SupplierPayment>
{
    Task<IQueryable<SupplierPayment>> GetBySupplierAsync(int supplierId, int? branchId = null);
    Task<SupplierPayment?> GetWithItemsAsync(int supplierPaymentId);
    Task<SupplierPaymentItem> AddPaymentItemAsync(SupplierPaymentItem item);
}