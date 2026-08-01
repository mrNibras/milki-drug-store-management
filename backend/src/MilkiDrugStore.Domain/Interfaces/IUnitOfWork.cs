using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;

namespace MilkiDrugStore.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Role> Roles { get; }
    IRepository<User> Users { get; }
    IRepository<Branch> Branches { get; }
    IRepository<Category> Categories { get; }
    IRepository<Medicine> Medicines { get; }
    IRepository<MedicineBatch> MedicineBatches { get; }
    IRepository<Supplier> Suppliers { get; }
    IRepository<Purchase> Purchases { get; }
    IRepository<PurchaseItem> PurchaseItems { get; }
    IRepository<Sale> Sales { get; }
    IRepository<SaleItem> SaleItems { get; }
    IRepository<InventoryTransaction> InventoryTransactions { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<Settings> Settings { get; }
    IRepository<DamageRecord> DamageRecords { get; }
    IRepository<ExpiredRecord> ExpiredRecords { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    IRepository<PasswordReset> PasswordResets { get; }
    IRepository<Cosmetic> Cosmetics { get; }
    IRepository<CosmeticBatch> CosmeticBatches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
