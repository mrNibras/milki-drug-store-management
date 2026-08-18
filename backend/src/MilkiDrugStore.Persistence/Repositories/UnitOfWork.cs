using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Roles = new Repository<Role>(_context);
        Users = new Repository<User>(_context);
        Branches = new Repository<Branch>(_context);
        Categories = new Repository<Category>(_context);
        Medicines = new Repository<Medicine>(_context);
        MedicineBatches = new Repository<MedicineBatch>(_context);
        Suppliers = new Repository<Supplier>(_context);
        Purchases = new Repository<Purchase>(_context);
        PurchaseItems = new Repository<PurchaseItem>(_context);
        Sales = new Repository<Sale>(_context);
        SaleItems = new Repository<SaleItem>(_context);
        InventoryTransactions = new Repository<InventoryTransaction>(_context);
        Notifications = new Repository<Notification>(_context);
        AuditLogs = new Repository<AuditLog>(_context);
        Settings = new Repository<Settings>(_context);
        DamageRecords = new Repository<DamageRecord>(_context);
        ExpiredRecords = new Repository<ExpiredRecord>(_context);
        RefreshTokens = new Repository<RefreshToken>(_context);
        PasswordResets = new Repository<PasswordReset>(_context);
        Cosmetics = new Repository<Cosmetic>(_context);
        CosmeticBatches = new Repository<CosmeticBatch>(_context);
    }

    public IRepository<Role> Roles { get; }
    public IRepository<User> Users { get; }
    public IRepository<Branch> Branches { get; }
    public IRepository<Category> Categories { get; }
    public IRepository<Medicine> Medicines { get; }
    public IRepository<MedicineBatch> MedicineBatches { get; }
    public IRepository<Supplier> Suppliers { get; }
    public IRepository<Purchase> Purchases { get; }
    public IRepository<PurchaseItem> PurchaseItems { get; }
    public IRepository<Sale> Sales { get; }
    public IRepository<SaleItem> SaleItems { get; }
    public IRepository<InventoryTransaction> InventoryTransactions { get; }
    public IRepository<Notification> Notifications { get; }
    public IRepository<AuditLog> AuditLogs { get; }
    public IRepository<Settings> Settings { get; }
    public IRepository<DamageRecord> DamageRecords { get; }
    public IRepository<ExpiredRecord> ExpiredRecords { get; }
    public IRepository<RefreshToken> RefreshTokens { get; }
    public IRepository<PasswordReset> PasswordResets { get; }
    public IRepository<Cosmetic> Cosmetics { get; }
    public IRepository<CosmeticBatch> CosmeticBatches { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
            throw new InvalidOperationException("A transaction is already active.");
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
