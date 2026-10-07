using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MilkiDrugStore.Application.DTOs.Medicine;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Integration.Database;

/// <summary>
/// Proves the supplier-cost redaction happens in the service layer,
/// by calling the service directly under an admin and a pharmacist
/// principal, each against a fresh DbContext.
/// </summary>
public class InventoryCostRedactionTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly ServiceProvider _serviceProvider;

    private int _branchId;
    private int _categoryId;
    private int _unitTypeId;
    private int _productId;
    private int _batchId;

    public InventoryCostRedactionTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(o =>
            o.UseNpgsql(_fixture.ConnectionString, n => n.MigrationsAssembly("MilkiDrugStore.Persistence")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IMedicineRepository, MedicineRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ICosmeticRepository, CosmeticRepository>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IMedicineService, MedicineService>();
        services.AddScoped<ICosmeticService, CosmeticService>();
        services.AddSingleton<IAuditLogService>(Mock.Of<IAuditLogService>());
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task InitializeAsync()
    {
        await InWriteScopeAsync(async db =>
        {
            await db.Database.MigrateAsync();

            _branchId = await EnsureAsync(db, db.Branches,
                () => new Branch { BranchName = "Redaction Branch", Location = "Test", IsActive = true },
                b => b.BranchName == "Redaction Branch");

            _categoryId = await EnsureAsync(db, db.Categories,
                () => new Category { Name = "Redaction Category", IsActive = true },
                c => c.Name == "Redaction Category");

            _unitTypeId = await EnsureAsync(db, db.UnitTypes,
                () => new UnitType { Name = "Redaction Unit", IsActive = true },
                u => u.Name == "Redaction Unit");

            _productId = await EnsureAsync(db, db.Medicines,
                () => new Medicine
                {
                    ProductCode = $"REDACT-{Guid.NewGuid():N}"[..24],
                    BrandName = "Redaction Amoxicillin",
                    GenericName = "Amoxicillin",
                    CategoryId = _categoryId,
                    UnitTypeId = _unitTypeId,
                    PurchasePrice = 60,
                    SellingPrice = 100,
                    ReorderLevel = 10,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                m => m.BrandName == "Redaction Amoxicillin");

            var batch = new MedicineBatch
            {
                ProductId = _productId,
                BranchId = _branchId,
                BatchNumber = $"REDACT-B-{Guid.NewGuid():N}"[..20],
                PurchasePrice = 60,
                SellingPrice = 100,
                QuantityReceived = 100,
                QuantityIssued = 0,
                QuantityDamaged = 0,
                QuantityExpired = 0,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                DateReceived = DateTime.UtcNow
            };
            db.MedicineBatches.Add(batch);
            await db.SaveChangesAsync();
            _batchId = batch.BatchId;
        });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task InWriteScopeAsync(Func<AppDbContext, Task> work)
    {
        using var scope = _serviceProvider.CreateScope();
        await work(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    /// <summary>
    /// Reads through a scope whose current user has the given role,
    /// so the role rule is applied to a real, freshly loaded entity.
    /// The service is constructed directly so the caller's role can be
    /// supplied explicitly, and the data comes from a fresh DbContext.
    /// </summary>
    private async Task<MedicineResponse> ReadAsAsync(string? roleName)
    {
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Only admins may see cost; a pharmacist, and any caller that
        // is not an admin, must not.
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.CanViewFinancialCosts).Returns(roleName == "Admin");

        var medicineRepo = services.GetRequiredService<IRepository<Medicine>>();
        var batchRepo = services.GetRequiredService<IRepository<MedicineBatch>>();
        var catalog = services.GetRequiredService<ICatalogService>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var auditLog = services.GetRequiredService<IAuditLogService>();

        var service = new MedicineService(
            medicineRepo, batchRepo, catalog, unitOfWork, auditLog, currentUser.Object);

        return await service.GetAllAsync().ContinueWith(t =>
            t.Result.Single(m => m.ProductId == _productId));
    }

    private static async Task<int> EnsureAsync<T>(AppDbContext db, DbSet<T> set, Func<T> factory, System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
    {
        var existing = await set.FirstOrDefaultAsync(predicate);
        if (existing != null)
            return GetId(existing)!.Value;

        var entity = factory();
        set.Add(entity);
        await db.SaveChangesAsync();
        return GetId(entity)!.Value;
    }

    private static int? GetId<T>(T entity) where T : class => entity switch
    {
        Branch b => b.BranchId,
        Category c => c.CategoryId,
        UnitType u => u.UnitTypeId,
        Medicine m => m.ProductId,
        _ => null
    };

    [Fact]
    public async Task Admin_Receives_PurchasePrice()
    {
        var response = await ReadAsAsync("Admin");

        var batch = response.Batches.Single(b => b.BatchId == _batchId);
        Assert.Equal(60m, response.PurchasePrice);
        Assert.Equal(60m, batch.PurchasePrice);
        Assert.Equal(100m, batch.SellingPrice);
    }

    [Fact]
    public async Task Pharmacist_DoesNotReceive_PurchasePrice_But_Keeps_SellingPrice()
    {
        var response = await ReadAsAsync("Pharmacist");

        var batch = response.Batches.Single(b => b.BatchId == _batchId);

        // Cost is withheld...
        Assert.Null(response.PurchasePrice);
        Assert.Null(batch.PurchasePrice);

        // ...while operational and selling data stays available for the POS.
        Assert.Equal(100m, batch.SellingPrice);
        Assert.Equal(100, batch.RemainingQuantity);
        Assert.Equal(batch.ExpiryDate, batch.ExpiryDate);
        Assert.Equal(response.BrandName, "Redaction Amoxicillin");
    }
}