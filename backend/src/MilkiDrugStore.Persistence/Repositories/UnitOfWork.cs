using System.Transactions;
using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private bool _transactionActive;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Roles = new Repository<Role>(_context);
        Users = new Repository<User>(_context);
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
    }

    public IRepository<Role> Roles { get; }
    public IRepository<User> Users { get; }
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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transactionActive = true;
        await Task.CompletedTask;
    }

    public async Task CommitTransactionAsync()
    {
        if (_transactionActive)
        {
            _transactionActive = false;
            await Task.CompletedTask;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transactionActive)
        {
            _transactionActive = false;
            await Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
