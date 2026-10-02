using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.DTOs.Purchase;
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
/// Verifies that Inventory resolves the supplier of a batch from the real
/// purchase/batch relationship in PostgreSQL.
///
/// The authoritative path is the batch's own supplier foreign key, which
/// PurchaseService populates from the purchase's supplier when a batch is
/// created through the purchase workflow. No supplier is denormalised onto the
/// product and no test seeds a supplier name directly: every assertion below
/// resolves it from the database.
/// </summary>
public class InventorySupplierTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly ServiceProvider _serviceProvider;
    private readonly IAuditLogService _auditLog = Mock.Of<IAuditLogService>();

    private int _branchId;
    private int _categoryId;
    private int _unitTypeId;
    private int _cosmeticCategoryId;
    private int _cosmeticUnitTypeId;
    private int _userId;

    public InventorySupplierTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(_fixture.ConnectionString, npgsql =>
                npgsql.MigrationsAssembly("MilkiDrugStore.Persistence")));

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
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddSingleton(_auditLog);
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Runs a unit of work against its own DI scope, i.e. its own DbContext.
    /// Seeding and asserting therefore never share a change tracker, so a
    /// navigation property is only populated if the query really loads it.
    /// </summary>
    private async Task<T> InNewScopeAsync<T>(Func<IServiceProvider, Task<T>> work)
    {
        using var scope = _serviceProvider.CreateScope();
        return await work(scope.ServiceProvider);
    }

    /// <summary>Runs a write against its own DbContext so nothing is left tracked.</summary>
    private async Task InNewDbScopeAsync(Func<AppDbContext, Task> work)
    {
        using var scope = _serviceProvider.CreateScope();
        await work(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    private Task<T> FromNewDbAsync<T>(Func<AppDbContext, Task<T>> work) =>
        InNewScopeAsync(sp => work(sp.GetRequiredService<AppDbContext>()));

    private Task<IEnumerable<MilkiDrugStore.Application.DTOs.Medicine.MedicineResponse>> FetchMedicinesAsync() =>
        InNewScopeAsync(sp => sp.GetRequiredService<IMedicineService>().GetAllAsync());

    private Task<IEnumerable<MilkiDrugStore.Application.DTOs.Cosmetic.CosmeticResponse>> FetchCosmeticsAsync() =>
        InNewScopeAsync(sp => sp.GetRequiredService<ICosmeticService>().GetAllAsync());

    private Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request) =>
        InNewScopeAsync(sp => sp.GetRequiredService<IPurchaseService>()
            .CreateAsync(request, _userId, _branchId));

    public async Task InitializeAsync()
    {
        await InNewDbScopeAsync(db => db.Database.MigrateAsync());

        _branchId = await EnsureAsync(
            () => new Branch { BranchName = "Supplier Test Branch", Location = "Test", IsActive = true },
            b => b.BranchName == "Supplier Test Branch");

        _categoryId = await EnsureAsync(
            () => new Category { Name = "Supplier Test Category", IsActive = true },
            c => c.Name == "Supplier Test Category");

        _unitTypeId = await EnsureAsync(
            () => new UnitType { Name = "Supplier Test Unit", IsActive = true },
            u => u.Name == "Supplier Test Unit");

        _cosmeticCategoryId = _categoryId;
        _cosmeticUnitTypeId = _unitTypeId;

        var roleId = await EnsureRoleAsync();
        var userEmail = $"suppliertest{_branchId}@example.com";
        _userId = await EnsureAsync(
            
            () => new User
            {
                FullName = "Supplier Test User",
                Email = userEmail,
                PasswordHash = "hash",
                RoleId = roleId,
                BranchId = _branchId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            u => u.Email == userEmail);
    }

    private async Task<int> EnsureRoleAsync()
    {
        return await FromNewDbAsync(async db =>
        {
            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (role != null)
                return role.RoleId;

            role = new Role { Name = "Admin" };
            db.Roles.Add(role);
            await db.SaveChangesAsync();
            return role.RoleId;
        });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ---------------------------------------------------------------------
    // Test 1 - single supplier
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_SingleSupplier_MedicineBatch_ResolvesSupplierName()
    {
        var supplier = await CreateSupplierAsync("Single Supplier Pharma");
        var medicine = await CreateMedicineAsync("SingleSupplierMed");
        var batch = await CreateMedicineBatchAsync(medicine, "SS-A001", supplier, 60, 100, 100);

        var result = (await FetchMedicinesAsync()).Single(m => m.ProductId == medicine.ProductId);
        var resolved = result.Batches.Single(b => b.BatchId == batch.BatchId);

        Assert.Equal(supplier.SupplierId, resolved.SupplierId);
        Assert.Equal(supplier.SupplierName, resolved.SupplierName);
    }

    // ---------------------------------------------------------------------
    // Test 2 - multiple batches, same supplier
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_MultipleBatchesSameSupplier_ResolvesSameSupplierOnEveryBatch()
    {
        var supplier = await CreateSupplierAsync("Shared Supplier Ltd");
        var medicine = await CreateMedicineAsync("SharedSupplierMed");
        var batchA = await CreateMedicineBatchAsync(medicine, "SS-B001", supplier, 60, 100, 100);
        var batchB = await CreateMedicineBatchAsync(medicine, "SS-B002", supplier, 65, 105, 50);

        var result = (await FetchMedicinesAsync()).Single(m => m.ProductId == medicine.ProductId);

        var names = result.Batches
            .Where(b => b.BatchId == batchA.BatchId || b.BatchId == batchB.BatchId)
            .Select(b => b.SupplierName)
            .ToList();

        Assert.Equal(2, names.Count);
        Assert.All(names, n => Assert.Equal(supplier.SupplierName, n));

        // The two batches collapse to a single distinct supplier for the summary.
        Assert.Single(names.Distinct());
    }

    // ---------------------------------------------------------------------
    // Test 3 - multiple batches, different suppliers (mandatory case)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_MultipleSuppliers_KeepsSupplierAssociationBatchSpecific()
    {
        var supplierA = await CreateSupplierAsync("Supplier A Pharma");
        var supplierB = await CreateSupplierAsync("Supplier B Medical");
        var medicine = await CreateMedicineAsync("MultiSupplierMed");

        var batchA = await CreateMedicineBatchAsync(medicine, "MS-A001", supplierA, 60, 100, 40);
        var batchB = await CreateMedicineBatchAsync(medicine, "MS-A002", supplierB, 70, 110, 30);

        var result = (await FetchMedicinesAsync()).Single(m => m.ProductId == medicine.ProductId);

        var resolvedA = result.Batches.Single(b => b.BatchId == batchA.BatchId);
        var resolvedB = result.Batches.Single(b => b.BatchId == batchB.BatchId);

        // Supplier association is per batch, never collapsed to one supplier.
        Assert.Equal(supplierA.SupplierName, resolvedA.SupplierName);
        Assert.Equal(supplierB.SupplierName, resolvedB.SupplierName);

        var distinctSuppliers = result.Batches
            .Where(b => b.BatchId == batchA.BatchId || b.BatchId == batchB.BatchId)
            .Select(b => b.SupplierName)
            .Distinct()
            .ToList();

        Assert.Equal(2, distinctSuppliers.Count);
        Assert.Contains(supplierA.SupplierName, distinctSuppliers);
        Assert.Contains(supplierB.SupplierName, distinctSuppliers);
    }

    // ---------------------------------------------------------------------
    // Test 4 - cosmetic supplier
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_CosmeticBatch_ResolvesSupplierName()
    {
        var supplierA = await CreateSupplierAsync("Cosmetic Supplier One");
        var supplierB = await CreateSupplierAsync("Cosmetic Supplier Two");
        var cosmetic = await CreateCosmeticAsync("MultiSupplierShampoo");

        var batchA = await CreateCosmeticBatchAsync(cosmetic, "CS-A001", supplierA, 55, 95, 20);
        var batchB = await CreateCosmeticBatchAsync(cosmetic, "CS-A002", supplierB, 75, 120, 10);

        var result = (await FetchCosmeticsAsync()).Single(c => c.CosmeticId == cosmetic.CosmeticId);

        var resolvedA = result.Batches.Single(b => b.BatchId == batchA.BatchId);
        var resolvedB = result.Batches.Single(b => b.BatchId == batchB.BatchId);

        Assert.Equal(supplierA.SupplierName, resolvedA.SupplierName);
        Assert.Equal(supplierB.SupplierName, resolvedB.SupplierName);

        var distinctSuppliers = result.Batches
            .Select(b => b.SupplierName)
            .Where(n => n != null)
            .Distinct()
            .ToList();

        Assert.Equal(2, distinctSuppliers.Count);
    }

    // ---------------------------------------------------------------------
    // Test 5 - null / missing supplier relationship must not crash
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_BatchWithoutSupplier_ReturnsNullNameAndDoesNotThrow()
    {
        var medicine = await CreateMedicineAsync("NoSupplierMed");
        var batch = await CreateMedicineBatchAsync(medicine, "NS-A001", supplier: null, 60, 100, 25);

        var result = (await FetchMedicinesAsync()).Single(m => m.ProductId == medicine.ProductId);
        var resolved = result.Batches.Single(b => b.BatchId == batch.BatchId);

        // A genuinely absent relationship surfaces as null, not a fabricated
        // placeholder, so the UI can distinguish "no supplier" from a bad query.
        Assert.Null(resolved.SupplierId);
        Assert.Null(resolved.SupplierName);
    }

    // ---------------------------------------------------------------------
    // Test 6 - purchase regression: the supplier comes from the purchase flow
    // ---------------------------------------------------------------------

    [Fact]
    public async Task CreatePurchase_Medicine_SetsBatchSupplierFromPurchase_AndInventoryResolvesIt()
    {
        var supplier = await CreateSupplierAsync("Purchase Flow Supplier");
        var medicine = await CreateMedicineAsync("PurchaseFlowMed");
        var batchNumber = $"PF-{Guid.NewGuid():N}"[..12];

        var purchase = await CreatePurchaseAsync(
            new CreatePurchaseRequest
            {
                SupplierId = supplier.SupplierId,
                PurchaseDate = DateTime.UtcNow,
                PaymentMethod = "cash",
                AmountPaid = 600,
                Items = new List<PurchaseItemRequest>
                {
                    new PurchaseItemRequest
                    {
                        ProductId = medicine.ProductId,
                        BatchNumber = batchNumber,
                        Quantity = 10,
                        PurchasePrice = 60,
                        SellingPrice = 100,
                        ExpiryDate = DateTime.UtcNow.AddYears(1)
                    }
                }
            });

        Assert.NotNull(purchase);

        // The purchase wrote the supplier onto the batch it created.
        var storedBatch = await FromNewDbAsync(db => db.MedicineBatches
            .AsNoTracking()
            .SingleAsync(b => b.BatchNumber == batchNumber));

        Assert.Equal(supplier.SupplierId, storedBatch.SupplierId);

        // And inventory resolves that same supplier from the database.
        var result = (await FetchMedicinesAsync()).Single(m => m.ProductId == medicine.ProductId);
        var resolved = result.Batches.Single(b => b.BatchId == storedBatch.BatchId);

        Assert.Equal(supplier.SupplierName, resolved.SupplierName);
        Assert.Equal(10, resolved.RemainingQuantity);
    }

    [Fact]
    public async Task CreatePurchase_Cosmetic_SetsBatchSupplierFromPurchase_AndInventoryResolvesIt()
    {
        var supplier = await CreateSupplierAsync("Cosmetic Purchase Supplier");
        var cosmetic = await CreateCosmeticAsync("PurchaseFlowShampoo");
        var batchNumber = $"CPF-{Guid.NewGuid():N}"[..12];

        var purchase = await CreatePurchaseAsync(
            new CreatePurchaseRequest
            {
                SupplierId = supplier.SupplierId,
                PurchaseDate = DateTime.UtcNow,
                PaymentMethod = "cash",
                AmountPaid = 550,
                Items = new List<PurchaseItemRequest>
                {
                    new PurchaseItemRequest
                    {
                        ProductType = "cosmetic",
                        ProductId = cosmetic.CosmeticId,
                        BatchNumber = batchNumber,
                        Quantity = 5,
                        PurchasePrice = 110,
                        SellingPrice = 150,
                        ExpiryDate = DateTime.UtcNow.AddYears(2)
                    }
                }
            });

        Assert.NotNull(purchase);

        var storedBatch = await FromNewDbAsync(db => db.CosmeticBatches
            .AsNoTracking()
            .SingleAsync(b => b.BatchNumber == batchNumber));

        Assert.Equal(supplier.SupplierId, storedBatch.SupplierId);

        var result = (await FetchCosmeticsAsync()).Single(c => c.CosmeticId == cosmetic.CosmeticId);
        var resolved = result.Batches.Single(b => b.BatchId == storedBatch.BatchId);

        Assert.Equal(supplier.SupplierName, resolved.SupplierName);
        Assert.Equal(5, resolved.Balance);
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    private Task<int> EnsureAsync<T>(Func<T> factory, System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
        => FromNewDbAsync(async db =>
        {
            var set = db.Set<T>();
            var existing = await set.FirstOrDefaultAsync(predicate);
            if (existing != null)
                return GetFallbackId(existing)!.Value;

            var entity = factory();
            set.Add(entity);
            await db.SaveChangesAsync();
            return GetFallbackId(entity)!.Value;
        });

    private static int? GetFallbackId(object entity) => entity switch
    {
        Branch b => b.BranchId,
        Category c => c.CategoryId,
        UnitType u => u.UnitTypeId,
        User u => u.UserId,
        _ => null
    };

    private async Task<Supplier> CreateSupplierAsync(string name)
    {
        // Unique per call so parallel/repeat runs never collide on the name.
        var uniqueName = $"{name} {Guid.NewGuid():N}"[..Math.Min(name.Length + 9, 60)];
        var supplier = new Supplier
        {
            SupplierName = uniqueName,
            Phone = "0000000000",
            Email = $"{Guid.NewGuid():N}@example.com",
            Address = "Test",
            CreatedAt = DateTime.UtcNow
        };
        await InNewDbScopeAsync(async db =>
        {
            db.Suppliers.Add(supplier);
            await db.SaveChangesAsync();
        });
        return supplier;
    }

    private async Task<Medicine> CreateMedicineAsync(string suffix)
    {
        var medicine = new Medicine
        {
            // Unique per call: ProductCode carries a unique index and the shared
            // test database may already hold rows from an earlier run.
            ProductCode = $"SUP{suffix}-{Guid.NewGuid():N}"[..28],
            BrandName = $"Supply Test {suffix}",
            GenericName = suffix,
            CategoryId = _categoryId,
            UnitTypeId = _unitTypeId,
            SellingPrice = 100,
            ReorderLevel = 10,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        await InNewDbScopeAsync(async db =>
        {
            db.Medicines.Add(medicine);
            await db.SaveChangesAsync();
        });
        return medicine;
    }

    private async Task<MedicineBatch> CreateMedicineBatchAsync(
        Medicine medicine, string batchNumber, Supplier? supplier,
        decimal purchasePrice, decimal sellingPrice, int quantity)
    {
        var batch = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = _branchId,
            BatchNumber = $"{batchNumber}-{Guid.NewGuid():N}"[..20],
            PurchasePrice = purchasePrice,
            SellingPrice = sellingPrice,
            QuantityReceived = quantity,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            DateReceived = DateTime.UtcNow,
            SupplierId = supplier?.SupplierId
        };
        await InNewDbScopeAsync(async db =>
        {
            db.MedicineBatches.Add(batch);
            await db.SaveChangesAsync();
        });
        return batch;
    }

    private async Task<Cosmetic> CreateCosmeticAsync(string suffix)
    {
        var cosmetic = new Cosmetic
        {
            ProductName = $"Supply Test {suffix}",
            Description = suffix,
            CategoryId = _cosmeticCategoryId,
            UnitTypeId = _cosmeticUnitTypeId,
            Price = 120,
            IsActive = true,
            BranchId = _branchId,
            CreatedAt = DateTime.UtcNow
        };
        await InNewDbScopeAsync(async db =>
        {
            db.Cosmetics.Add(cosmetic);
            await db.SaveChangesAsync();
        });
        return cosmetic;
    }

    private async Task<CosmeticBatch> CreateCosmeticBatchAsync(
        Cosmetic cosmetic, string batchNumber, Supplier supplier,
        decimal buyingPrice, decimal sellingPrice, int quantity)
    {
        var batch = new CosmeticBatch
        {
            CosmeticId = cosmetic.CosmeticId,
            BranchId = _branchId,
            BatchNumber = $"{batchNumber}-{Guid.NewGuid():N}"[..20],
            QuantityReceived = quantity,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            ExpiryDate = DateTime.UtcNow.AddYears(2),
            DateReceived = DateTime.UtcNow,
            BuyingPrice = buyingPrice,
            SellingPrice = sellingPrice,
            LowStockThreshold = 5,
            SupplierId = supplier.SupplierId
        };
        await InNewDbScopeAsync(async db =>
        {
            db.CosmeticBatches.Add(batch);
            await db.SaveChangesAsync();
        });
        return batch;
    }

}
