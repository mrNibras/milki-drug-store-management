using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

namespace MilkiDrugStore.Tests.Integration.Persistence;

public class PersistenceTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;

    public PersistenceTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(_fixture.ConnectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("MilkiDrugStore.Persistence");
            }));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IMedicineRepository, MedicineRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.MigrateAsync();

        if (!await _dbContext.Roles.AnyAsync())
        {
            _dbContext.Branches.Add(new Branch { BranchName = "Persistence Branch", Location = "Test", IsActive = true, CreatedAt = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync();

            _dbContext.Roles.AddRange(new[] { new Role { Name = "Admin" }, new Role { Name = "Pharmacist" } });
            await _dbContext.SaveChangesAsync();

            _dbContext.Categories.Add(new Category { Name = "Persistence Category", IsActive = true, CreatedAt = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync();

            _dbContext.UnitTypes.Add(new UnitType { Name = "Bottle", IsActive = true });
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (_dbContext != null)
            {
                await _dbContext.Database.ExecuteSqlRawAsync("DROP SCHEMA public CASCADE; CREATE SCHEMA public;");
                await _dbContext.DisposeAsync();
            }
        }
        catch { }

        if (_serviceProvider is IAsyncDisposable ad)
            await ad.DisposeAsync();
        else if (_serviceProvider is IDisposable d)
            d.Dispose();
    }

    private AppDbContext FreshContext()
    {
        return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_fixture.ConnectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("MilkiDrugStore.Persistence");
            })
            .Options);
    }

    [Fact]
    public async Task Medicine_Should_Survive_Context_Recycle()
    {
        var category = await _dbContext.Categories.FirstAsync();
        var unitType = await _dbContext.UnitTypes.FirstAsync();

        var medicine = new Medicine
        {
            BrandName = "Persistence Medicine 001",
            GenericName = "Persistence Generic",
            CategoryId = category.CategoryId,
            UnitTypeId = unitType.UnitTypeId,
            ReorderLevel = 10,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var medicineId = medicine.ProductId;

        await using var fresh = FreshContext();
        var saved = await fresh.Medicines.FirstOrDefaultAsync(m => m.ProductId == medicineId);
        Assert.NotNull(saved);
        Assert.Equal("Persistence Generic", saved!.GenericName);
    }

    [Fact]
    public async Task User_Should_Survive_Context_Recycle()
    {
        var role = await _dbContext.Roles.FirstAsync(r => r.Name == "Pharmacist");
        var branch = await _dbContext.Branches.FirstAsync();
        var email = $"persistence-{Guid.NewGuid():N}@test.local";

        var user = new User
        {
            FullName = "Persistence Pharmacist",
            Email = email,
            PasswordHash = "hash",
            RoleId = role.RoleId,
            BranchId = branch.BranchId,
            IsApproved = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var userId = user.UserId;

        await using var fresh = FreshContext();
        var saved = await fresh.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        Assert.NotNull(saved);
        Assert.Equal("Persistence Pharmacist", saved!.FullName);
    }

    [Fact]
    public async Task Supplier_Should_Survive_Context_Recycle()
    {
        _dbContext.Suppliers.Add(new Supplier
        {
            SupplierName = "Persistence Supplier 001",
            Phone = "0911111111",
            Email = $"supplier-{Guid.NewGuid():N}@test.local",
            Address = "Persistence Address",
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.SupplierName == "Persistence Supplier 001");
        var supplierId = supplier!.SupplierId;

        await using var fresh = FreshContext();
        var saved = await fresh.Suppliers.FirstOrDefaultAsync(s => s.SupplierId == supplierId);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Sale_With_Items_Should_Survive_Context_Recycle()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var role = await _dbContext.Roles.FirstAsync(r => r.Name == "Pharmacist");
        var category = await _dbContext.Categories.FirstAsync();
        var unitType = await _dbContext.UnitTypes.FirstAsync();

        var pharmacist = new User
        {
            FullName = "Persistence Sales Pharmacist",
            Email = $"saleuser-{Guid.NewGuid():N}@test.local",
            PasswordHash = "hash",
            RoleId = role.RoleId,
            BranchId = branch.BranchId,
            IsApproved = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(pharmacist);
        await _dbContext.SaveChangesAsync();

        var medicine = new Medicine
        {
            BrandName = "Persistence Sale Medicine",
            GenericName = "Persistence Sale Generic",
            CategoryId = category.CategoryId,
            UnitTypeId = unitType.UnitTypeId,
            ReorderLevel = 5,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var batch = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = branch.BranchId,
            BatchNumber = $"SALE-BATCH-{Guid.NewGuid():N}",
            PurchasePrice = 30,
            SellingPrice = 60,
            QuantityReceived = 20,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            DateReceived = DateTime.UtcNow
        };
        _dbContext.MedicineBatches.Add(batch);
        await _dbContext.SaveChangesAsync();

        var sale = new Sale
        {
            SaleNumber = $"SAL-{Guid.NewGuid():N}",
            SaleDate = DateTime.UtcNow,
            TotalAmount = 120,
            TotalProfit = 60,
            UserId = pharmacist.UserId,
            BranchId = branch.BranchId,
            PaymentMethod = "cash",
            PaymentStatus = "paid",
            AmountPaid = 120,
            AmountDue = 0
        };
        _dbContext.Sales.Add(sale);
        await _dbContext.SaveChangesAsync();

        _dbContext.SaleItems.Add(new SaleItem
        {
            SaleId = sale.SaleId,
            ProductId = medicine.ProductId,
            BatchId = batch.BatchId,
            Quantity = 2,
            UnitPrice = 60,
            PurchasePrice = 30,
            Profit = 60,
            SubTotal = 120
        });
        await _dbContext.SaveChangesAsync();

        var saleId = sale.SaleId;

        await using var fresh = FreshContext();
        var savedSale = await fresh.Sales.FirstOrDefaultAsync(s => s.SaleId == saleId);
        Assert.NotNull(savedSale);
        Assert.Equal(branch.BranchId, savedSale!.BranchId);
        Assert.Equal("paid", savedSale.PaymentStatus);

        var savedItem = await fresh.SaleItems.FirstOrDefaultAsync(si => si.SaleId == saleId);
        Assert.NotNull(savedItem);
        Assert.Equal(2, savedItem!.Quantity);
        Assert.Equal(120, savedItem.SubTotal);
    }

    [Fact]
    public async Task Purchase_With_Batch_And_Transaction_Should_Survive_Context_Recycle()
    {
        var branch = await _dbContext.Branches.FirstAsync();

        var supplier = new Supplier
        {
            SupplierName = "Persistence Purchase Supplier",
            Phone = "0922222222",
            Email = $"psupplier-{Guid.NewGuid():N}@test.local",
            Address = "Addr",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Suppliers.Add(supplier);
        await _dbContext.SaveChangesAsync();

        var category = await _dbContext.Categories.FirstAsync();
        var unitType = await _dbContext.UnitTypes.FirstAsync();

        var catalogMock = new Mock<ICatalogService>();
        catalogMock.Setup(m => m.ResolveCategoryIdAsync(It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync(category.CategoryId);
        catalogMock.Setup(m => m.ResolveUnitTypeIdAsync(It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync(unitType.UnitTypeId);
        catalogMock.Setup(m => m.IsValidCategoryIdAsync(It.IsAny<int>())).ReturnsAsync(true);
        catalogMock.Setup(m => m.IsValidUnitTypeIdAsync(It.IsAny<int>())).ReturnsAsync(true);
        catalogMock.Setup(m => m.GetCategoryNameAsync(It.IsAny<int>())).ReturnsAsync("Persistence Category");

        var service = new PurchaseService(
            new Repository<Purchase>(_dbContext),
            new Repository<PurchaseItem>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new Repository<Supplier>(_dbContext),
            catalogMock.Object,
            new Repository<InventoryTransaction>(_dbContext),
            new UnitOfWork(_dbContext),
            Mock.Of<IAuditLogService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PurchaseService>>(), new CosmeticRepository(_dbContext), new Repository<CosmeticBatch>(_dbContext));

        var batchNumber = $"BATCH-{Guid.NewGuid():N}";
        var result = await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 1000,
            Items = new List<PurchaseItemRequest>
            {
                new()
                {
                    BrandName = "Auto Persistence Medicine",
                    CategoryName = "Persistence Category",
                    UnitType = "Bottle",
                    BatchNumber = batchNumber,
                    Quantity = 50,
                    PurchasePrice = 20,
                    SellingPrice = 40,
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                }
            }
        }, 1, branch.BranchId);

        var purchaseId = result.PurchaseId;

        await using var fresh = FreshContext();
        var purchase = await fresh.Purchases.FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);
        Assert.NotNull(purchase);
        Assert.Equal(branch.BranchId, purchase!.BranchId);

        var purchaseItem = await fresh.PurchaseItems.FirstOrDefaultAsync(pi => pi.PurchaseId == purchaseId);
        Assert.NotNull(purchaseItem);

        var batch = await fresh.MedicineBatches.FirstOrDefaultAsync(b => b.BatchNumber == batchNumber);
        Assert.NotNull(batch);

        var transaction = await fresh.InventoryTransactions.FirstOrDefaultAsync(t => t.BatchId == batch!.BatchId);
        Assert.NotNull(transaction);
        Assert.Equal("Purchase", transaction!.TransactionType);
    }
}
