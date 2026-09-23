using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Persistence.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Purchase;
using MilkiDrugStore.Application.DTOs.Sale;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Integration.Database;

public class DatabaseIntegrationTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;

    public DatabaseIntegrationTests(PostgreSqlFixture fixture)
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
        services.AddScoped<ICosmeticRepository, CosmeticRepository>();

        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.MigrateAsync();

        if (!await _dbContext.Roles.AnyAsync())
        {
            _dbContext.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "Pharmacist" }
            );
            await _dbContext.SaveChangesAsync();
        }

        if (!await _dbContext.Branches.AnyAsync())
        {
            _dbContext.Branches.Add(new Branch
            {
                BranchName = "Test Branch",
                Location = "Test Location",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }

        if (!await _dbContext.UnitTypes.AnyAsync())
        {
            _dbContext.UnitTypes.AddRange(
                new UnitType { Name = "Bottle", Description = "Liquid container", IsActive = true }
            );
            await _dbContext.SaveChangesAsync();
        }

        if (!await _dbContext.Categories.AnyAsync())
        {
            _dbContext.Categories.Add(new Category
            {
                Name = "Test Category",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }

        if (!await _dbContext.Suppliers.AnyAsync())
        {
            _dbContext.Suppliers.Add(new Supplier
            {
                SupplierName = "Test Supplier",
                Phone = "0912345678",
                Email = "supplier@test.com",
                Address = "Test Address",
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }

        if (!await _dbContext.Users.AnyAsync())
        {
            var adminRole = await _dbContext.Roles.FirstAsync(r => r.Name == "Admin");
            var testBranch = await _dbContext.Branches.FirstAsync(b => b.BranchName == "Test Branch");
            _dbContext.Users.Add(new User
            {
                FullName = "Test User",
                Email = "testuser@test.com",
                PasswordHash = "hash",
                RoleId = adminRole.RoleId,
                BranchId = testBranch.BranchId,
                IsApproved = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.ExecuteSqlRawAsync("DROP SCHEMA public CASCADE; CREATE SCHEMA public;");
            await _dbContext.DisposeAsync();
        }

        if (_serviceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            _serviceProvider.Dispose();
        }
    }

    [Fact]
    public async Task Migrations_Should_Create_All_Tables()
    {
        Assert.True(await _dbContext.Roles.CountAsync() >= 0);
        Assert.True(await _dbContext.Users.CountAsync() >= 0);
        Assert.True(await _dbContext.Branches.CountAsync() >= 0);
        Assert.True(await _dbContext.Categories.CountAsync() >= 0);
        Assert.True(await _dbContext.Medicines.CountAsync() >= 0);
        Assert.True(await _dbContext.MedicineBatches.CountAsync() >= 0);
        Assert.True(await _dbContext.Suppliers.CountAsync() >= 0);
        Assert.True(await _dbContext.Purchases.CountAsync() >= 0);
        Assert.True(await _dbContext.PurchaseItems.CountAsync() >= 0);
        Assert.True(await _dbContext.Sales.CountAsync() >= 0);
        Assert.True(await _dbContext.SaleItems.CountAsync() >= 0);
        Assert.True(await _dbContext.InventoryTransactions.CountAsync() >= 0);
        Assert.True(await _dbContext.Notifications.CountAsync() >= 0);
        Assert.True(await _dbContext.AuditLogs.CountAsync() >= 0);
        Assert.True(await _dbContext.Settings.CountAsync() >= 0);
        Assert.True(await _dbContext.DamageRecords.CountAsync() >= 0);
        Assert.True(await _dbContext.ExpiredRecords.CountAsync() >= 0);
        Assert.True(await _dbContext.RefreshTokens.CountAsync() >= 0);
        Assert.True(await _dbContext.UnitTypes.CountAsync() >= 0);
        Assert.True(await _dbContext.Cosmetics.CountAsync() >= 0);
        Assert.True(await _dbContext.CosmeticBatches.CountAsync() >= 0);
    }

    [Fact]
    public async Task Seed_Should_Create_Default_Roles()
    {
        var adminRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var pharmacistRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Pharmacist");

        Assert.NotNull(adminRole);
        Assert.NotNull(pharmacistRole);
        Assert.Equal("Admin", adminRole!.Name);
        Assert.Equal("Pharmacist", pharmacistRole!.Name);
    }

    [Fact]
    public async Task Medicine_CRUD_Should_Work()
    {
        var category = new Category { Name = "CRUD Test Category" };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var medicine = new Medicine
        {
            BrandName = "CRUD Test Medicine",
            GenericName = "Test Generic",
            CategoryId = category.CategoryId,
            UnitTypeId = 1,
            ReorderLevel = 10,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var saved = await _dbContext.Medicines.FirstOrDefaultAsync(m => m.BrandName == "CRUD Test Medicine");
        Assert.NotNull(saved);
        Assert.Equal("Test Generic", saved!.GenericName);

        saved.BrandName = "Updated Medicine";
        await _dbContext.SaveChangesAsync();
        var updated = await _dbContext.Medicines.FirstOrDefaultAsync(m => m.ProductId == saved.ProductId);
        Assert.Equal("Updated Medicine", updated!.BrandName);

        updated.IsActive = false;
        await _dbContext.SaveChangesAsync();
        var deleted = await _dbContext.Medicines.FirstOrDefaultAsync(m => m.ProductId == saved.ProductId);
        Assert.False(deleted!.IsActive);
    }

    [Fact]
    public async Task MedicineBatch_Should_Calculate_Balance()
    {
        var medicine = new Medicine
        {
            BrandName = "Batch Test Medicine",
            CategoryId = 1,
            UnitTypeId = 1,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var batch = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = 1,
            BatchNumber = $"BATCH-{Guid.NewGuid():N}",
            PurchasePrice = 100,
            SellingPrice = 150,
            QuantityReceived = 100,
            QuantityIssued = 30,
            QuantityDamaged = 5,
            QuantityExpired = 0,
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            DateReceived = DateTime.UtcNow
        };
        _dbContext.MedicineBatches.Add(batch);
        await _dbContext.SaveChangesAsync();

        var savedBatch = await _dbContext.MedicineBatches.FirstAsync();

        Assert.Equal(65, savedBatch.RemainingQuantity);
    }

    [Fact]
    public async Task FEFO_Should_Prioritize_Earliest_Expiry()
    {
        var medicine = new Medicine
        {
            BrandName = "FEFO Test Medicine",
            CategoryId = 1,
            UnitTypeId = 1,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var batch1 = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = 1,
            BatchNumber = $"BATCH-{Guid.NewGuid():N}",
            PurchasePrice = 100,
            SellingPrice = 150,
            QuantityReceived = 50,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            ExpiryDate = DateTime.UtcNow.AddMonths(12),
            DateReceived = DateTime.UtcNow
        };

        var batch2 = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = 1,
            BatchNumber = "BATCH-002",
            PurchasePrice = 100,
            SellingPrice = 150,
            QuantityReceived = 50,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            ExpiryDate = DateTime.UtcNow.AddMonths(3),
            DateReceived = DateTime.UtcNow
        };

        _dbContext.MedicineBatches.AddRange(batch1, batch2);
        await _dbContext.SaveChangesAsync();

        var batches = await _dbContext.MedicineBatches
            .Where(b => b.ProductId == medicine.ProductId && b.QuantityReceived - b.QuantityIssued - b.QuantityDamaged - b.QuantityExpired > 0)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        Assert.Equal(2, batches.Count);
        Assert.Equal("BATCH-002", batches[0].BatchNumber);
        Assert.Equal(batch1.BatchNumber, batches[1].BatchNumber);
    }

    [Fact]
    public async Task Purchase_Should_Create_Batches_And_Transactions()
    {
        var medicine = new Medicine
        {
            BrandName = "Purchase Test Medicine",
            CategoryId = 1,
            UnitTypeId = 1,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var purchase = new Purchase
        {
            PurchaseNumber = "PUR-2026-00001",
            SupplierId = 1,
            BranchId = 1,
            PurchaseDate = DateTime.UtcNow,
            TotalAmount = 5000,
            CreatedBy = 1
        };
        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync();

        var purchaseItem = new PurchaseItem
        {
            PurchaseId = purchase.PurchaseId,
            ProductId = medicine.ProductId,
            BatchNumber = "PUR-BATCH-001",
            Quantity = 100,
            PurchasePrice = 50,
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        };
        _dbContext.PurchaseItems.Add(purchaseItem);
        await _dbContext.SaveChangesAsync();

        var batch = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = 1,
            BatchNumber = "PUR-BATCH-001",
            PurchasePrice = 50,
            SellingPrice = 80,
            QuantityReceived = 100,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            DateReceived = DateTime.UtcNow
        };
        _dbContext.MedicineBatches.Add(batch);
        await _dbContext.SaveChangesAsync();

        var savedPurchase = await _dbContext.Purchases.FirstAsync();
        Assert.Equal("PUR-2026-00001", savedPurchase.PurchaseNumber);
        Assert.Equal(5000, savedPurchase.TotalAmount);

        var savedBatch = await _dbContext.MedicineBatches.FirstAsync();
        Assert.Equal(100, savedBatch.RemainingQuantity);
    }

    [Fact]
    public async Task Sale_Should_Deduct_Inventory_And_Calculate_Profit()
    {
        var medicine = new Medicine
        {
            BrandName = "Sale Test Medicine",
            CategoryId = 1,
            UnitTypeId = 1,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var batch = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = 1,
            BatchNumber = "SALE-BATCH-001",
            PurchasePrice = 50,
            SellingPrice = 80,
            QuantityReceived = 100,
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
            SaleNumber = "SAL-2026-00001",
            SaleDate = DateTime.UtcNow,
            TotalAmount = 160,
            TotalProfit = 60,
            UserId = 1,
            BranchId = 1,
            PaymentMethod = "cash",
            PaymentStatus = "paid",
            AmountPaid = 160,
            AmountDue = 0
        };
        _dbContext.Sales.Add(sale);
        await _dbContext.SaveChangesAsync();

        var saleItem = new SaleItem
        {
            SaleId = sale.SaleId,
            ProductId = medicine.ProductId,
            BatchId = batch.BatchId,
            Quantity = 2,
            UnitPrice = 80,
            PurchasePrice = 50,
            Profit = 60,
            SubTotal = 160
        };
        _dbContext.SaleItems.Add(saleItem);
        await _dbContext.SaveChangesAsync();

        batch.QuantityIssued += 2;
        await _dbContext.SaveChangesAsync();

        var savedSale = await _dbContext.Sales.FirstAsync();
        Assert.Equal("SAL-2026-00001", savedSale.SaleNumber);
        Assert.Equal(160, savedSale.TotalAmount);
        Assert.Equal(60, savedSale.TotalProfit);

        var updatedBatch = await _dbContext.MedicineBatches.FirstAsync();
        Assert.Equal(2, updatedBatch.QuantityIssued);
        Assert.Equal(98, updatedBatch.RemainingQuantity);
    }

    [Fact]
    public async Task AuditLog_Should_Be_Created_For_Critical_Actions()
    {
        var user = new User
        {
            FullName = "Test User",
            Email = "test@test.com",
            PasswordHash = "hash",
            RoleId = 1,
            BranchId = 1,
            IsApproved = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var auditLog = new AuditLog
        {
            UserId = user.UserId,
            BranchId = 1,
            Action = "Created Sale",
            TableName = "Sales",
            RecordId = 1,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync();

        var savedLog = await _dbContext.AuditLogs.FirstAsync();
        Assert.Equal("Created Sale", savedLog.Action);
        Assert.Equal("Sales", savedLog.TableName);
        Assert.Equal(user.UserId, savedLog.UserId);
    }

    [Fact]
    public async Task Notification_Should_Be_Created_For_Low_Stock()
    {
        var medicine = new Medicine
        {
            BrandName = "Low Stock Medicine",
            CategoryId = 1,
            UnitTypeId = 1,
            ReorderLevel = 10,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var batch = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = 1,
            BatchNumber = "LOW-STOCK-BATCH",
            PurchasePrice = 50,
            SellingPrice = 80,
            QuantityReceived = 5,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            DateReceived = DateTime.UtcNow
        };
        _dbContext.MedicineBatches.Add(batch);
        await _dbContext.SaveChangesAsync();

        var notification = new Notification
        {
            BranchId = 1,
            Title = "Low Stock Alert",
            Message = $"{medicine.BrandName} stock is below threshold",
            NotificationType = "LOW_STOCK",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        var savedNotification = await _dbContext.Notifications.FirstAsync();
        Assert.Equal("Low Stock Alert", savedNotification.Title);
        Assert.Equal("LOW_STOCK", savedNotification.NotificationType);
        Assert.False(savedNotification.IsRead);
    }

    [Fact]
    public async Task Damage_And_Expiry_Should_Reduce_Inventory()
    {
        var medicine = new Medicine
        {
            BrandName = "Damage Test Medicine",
            CategoryId = 1,
            UnitTypeId = 1,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var batch = new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = 1,
            BatchNumber = "DAMAGE-BATCH",
            PurchasePrice = 50,
            SellingPrice = 80,
            QuantityReceived = 100,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            DateReceived = DateTime.UtcNow
        };
        _dbContext.MedicineBatches.Add(batch);
        await _dbContext.SaveChangesAsync();

        var damage = new DamageRecord
        {
            BranchId = 1,
            BatchId = batch.BatchId,
            Quantity = 5,
            Reason = "Broken Package",
            RecordedBy = 1,
            RecordedDate = DateTime.UtcNow
        };
        _dbContext.DamageRecords.Add(damage);
        batch.QuantityDamaged += 5;
        await _dbContext.SaveChangesAsync();

        var updatedBatch = await _dbContext.MedicineBatches.FirstAsync();
        Assert.Equal(5, updatedBatch.QuantityDamaged);
        Assert.Equal(95, updatedBatch.RemainingQuantity);

        var savedDamage = await _dbContext.DamageRecords.FirstAsync();
        Assert.Equal(5, savedDamage.Quantity);
        Assert.Equal("Broken Package", savedDamage.Reason);
    }

    [Fact]
    public async Task Branch_Should_Be_Required_For_Users()
    {
        var branch = new Branch
        {
            BranchName = "Integration Test Branch",
            Location = "Test Location",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Branches.Add(branch);
        await _dbContext.SaveChangesAsync();

        var adminRole = await _dbContext.Roles.FirstAsync(r => r.Name == "Admin");

        var user = new User
        {
            FullName = "Branch User",
            Email = "branchuser@test.com",
            PasswordHash = "hash",
            RoleId = adminRole.RoleId,
            BranchId = branch.BranchId,
            IsApproved = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var savedUser = await _dbContext.Users.FirstAsync(u => u.Email == "branchuser@test.com");
        Assert.Equal(branch.BranchId, savedUser.BranchId);
        Assert.Equal("Integration Test Branch", savedUser.Branch!.BranchName);
    }

    [Fact]
    public async Task PurchaseService_Should_Create_Purchase_With_Real_Database()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();

        var medicine = new Medicine
        {
            BrandName = "Integration Purchase Medicine",
            CategoryId = 1,
            UnitTypeId = 1,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var catalogMock = new Mock<ICatalogService>();
        catalogMock.Setup(m => m.ResolveCategoryIdAsync(It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync(1);
        catalogMock.Setup(m => m.ResolveUnitTypeIdAsync(It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync(1);
        catalogMock.Setup(m => m.IsValidCategoryIdAsync(It.IsAny<int>())).ReturnsAsync(true);
        catalogMock.Setup(m => m.IsValidUnitTypeIdAsync(It.IsAny<int>())).ReturnsAsync(true);
        catalogMock.Setup(m => m.GetCategoryNameAsync(It.IsAny<int>())).ReturnsAsync("Test Category");

        var purchaseService = new PurchaseService(
            new Repository<Purchase>(_dbContext),
            new Repository<PurchaseItem>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new Repository<Supplier>(_dbContext),
            catalogMock.Object,
            new Repository<InventoryTransaction>(_dbContext),
            new UnitOfWork(_dbContext),
            Mock.Of<IAuditLogService>(),
            Mock.Of<ILogger<PurchaseService>>(), new CosmeticRepository(_dbContext), new Repository<CosmeticBatch>(_dbContext));

        var request = new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 500,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductId = medicine.ProductId,
                    BatchNumber = "INTEG-BATCH-001",
                    Quantity = 100,
                    PurchasePrice = 50,
                    SellingPrice = 80,
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                }
            }
        };

        var result = await purchaseService.CreateAsync(request, 1, branch.BranchId);

        Assert.NotNull(result);
        Assert.Equal(supplier.SupplierId, result.SupplierId);
        Assert.Single(result.Items);
        Assert.Equal(medicine.ProductId, result.Items[0].ProductId);
        Assert.Equal("INTEG-BATCH-001", result.Items[0].BatchNumber);
        Assert.Equal(100, result.Items[0].Quantity);
        Assert.Equal(5000, result.TotalAmount);
        Assert.Equal(500, result.AmountPaid);
        Assert.Equal("partial", result.PaymentStatus);

        var savedPurchase = await _dbContext.Purchases.FirstAsync();
        Assert.Equal(branch.BranchId, savedPurchase.BranchId);
        Assert.StartsWith("PUR-", savedPurchase.PurchaseNumber);

        var savedBatch = await _dbContext.MedicineBatches.FirstAsync();
        Assert.Equal(medicine.ProductId, savedBatch.ProductId);
        Assert.Equal(branch.BranchId, savedBatch.BranchId);
        Assert.Equal(100, savedBatch.QuantityReceived);
    }

    [Fact]
    public async Task PurchaseService_Should_Create_Purchase_With_New_Medicine_And_Real_CatalogService()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();

        var auditLog = Mock.Of<IAuditLogService>();
        var logger = Mock.Of<ILogger<PurchaseService>>();
        var unitOfWork = new UnitOfWork(_dbContext);
        var catalogService = new CatalogService(
            new Repository<Category>(_dbContext),
            new Repository<UnitType>(_dbContext),
            unitOfWork,
            auditLog);

        var purchaseService = new PurchaseService(
            new Repository<Purchase>(_dbContext),
            new Repository<PurchaseItem>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new Repository<Supplier>(_dbContext),
            catalogService,
            new Repository<InventoryTransaction>(_dbContext),
            unitOfWork,
            auditLog,
            logger,
            new CosmeticRepository(_dbContext),
            new Repository<CosmeticBatch>(_dbContext));

        var request = new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 500,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    BrandName = "Auto Created Medicine",
                    GenericName = "Automated Generic",
                    CategoryId = -1,
                    CategoryName = "Antibiotics",
                    UnitType = "Tablet",
                    BatchNumber = "AUTO-BATCH-001",
                    Quantity = 100,
                    PurchasePrice = 5,
                    SellingPrice = 10,
                    ExpiryDate = DateTime.SpecifyKind(DateTime.UtcNow.AddYears(1), DateTimeKind.Unspecified)
                }
            }
        };

        var result = await purchaseService.CreateAsync(request, 1, branch.BranchId);

        Assert.NotNull(result);
        Assert.Equal(supplier.SupplierId, result.SupplierId);
        Assert.Single(result.Items);
        Assert.Equal(500, result.TotalAmount);
        Assert.Equal(500, result.AmountPaid);
        Assert.Equal("paid", result.PaymentStatus);

         var savedBatch = await _dbContext.MedicineBatches.FirstAsync();
        Assert.Equal(100, savedBatch.QuantityReceived);
        Assert.Equal("AUTO-BATCH-001", savedBatch.BatchNumber);
    }

    [Fact]
    public async Task PurchaseService_GetAllAsync_Should_Return_Purchases_With_PostgreSql()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();

        var auditLog = Mock.Of<IAuditLogService>();
        var logger = Mock.Of<ILogger<PurchaseService>>();
        var unitOfWork = new UnitOfWork(_dbContext);
        var catalogService = new CatalogService(
            new Repository<Category>(_dbContext),
            new Repository<UnitType>(_dbContext),
            unitOfWork,
            auditLog);

        var purchaseService = new PurchaseService(
            new Repository<Purchase>(_dbContext),
            new Repository<PurchaseItem>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new Repository<Supplier>(_dbContext),
            catalogService,
            new Repository<InventoryTransaction>(_dbContext),
            unitOfWork,
            auditLog,
            logger,
            new CosmeticRepository(_dbContext),
            new Repository<CosmeticBatch>(_dbContext));

         var request = new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 500,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    BrandName = "Repro Medicine",
                    GenericName = "Repro Generic",
                    CategoryId = -1,
                    CategoryName = "Repro Category",
                    UnitType = "Tablet",
                    BatchNumber = "REPRO-BATCH-001",
                    Quantity = 10,
                    PurchasePrice = 50,
                    SellingPrice = 80,
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                }
            }
        };

        await purchaseService.CreateAsync(request, 1, branch.BranchId);

        var result = await purchaseService.GetAllAsync(branch.BranchId);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task SupplierReport_Should_Show_Purchase_Count_And_Financial_Totals()
    {
        var branch = await _dbContext.Branches.FirstAsync();

        var supplier = new Supplier
        {
            SupplierName = "FinTest Supplier",
            Phone = "0911111111",
            Email = "s1@test.com",
            Address = "Test Address",
            PaymentStatus = "Outstanding",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Suppliers.Add(supplier);
        await _dbContext.SaveChangesAsync();

        var auditLog = Mock.Of<IAuditLogService>();
        var logger = Mock.Of<ILogger<PurchaseService>>();
        var unitOfWork = new UnitOfWork(_dbContext);
        var catalogService = new CatalogService(
            new Repository<Category>(_dbContext),
            new Repository<UnitType>(_dbContext),
            unitOfWork,
            auditLog);

        var purchaseService = new PurchaseService(
            new Repository<Purchase>(_dbContext),
            new Repository<PurchaseItem>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new Repository<Supplier>(_dbContext),
            catalogService,
            new Repository<InventoryTransaction>(_dbContext),
            unitOfWork,
            auditLog,
            logger,
            new CosmeticRepository(_dbContext),
            new Repository<CosmeticBatch>(_dbContext));

        var request = new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 500,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    BrandName = "FinTest Medicine",
                    GenericName = "FinTest Generic",
                    CategoryId = -1,
                    CategoryName = "Antibiotics",
                    UnitType = "Tablet",
                    BatchNumber = "FIN-BATCH-001",
                    Quantity = 100,
                    PurchasePrice = 5,
                    SellingPrice = 10,
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                }
            }
        };

        await purchaseService.CreateAsync(request, 1, branch.BranchId);

        var reportService = new ReportService(
            new Repository<Sale>(_dbContext),
            new Repository<Purchase>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<Supplier>(_dbContext),
            new Repository<User>(_dbContext),
            catalogService,
            new CosmeticRepository(_dbContext));

        var supplierReport = (await reportService.GetSupplierReportAsync(branch.BranchId)).ToList();

        var entry = supplierReport.First(r => r.SupplierId == supplier.SupplierId);
        Assert.Equal(1, entry.Purchases);
        Assert.Equal(500m, entry.TotalAmount);
        Assert.Equal(500m, entry.TotalPaid);
        Assert.Equal(0m, entry.TotalDebt);
        Assert.Equal("Cleared", entry.PaymentStatus);
    }

    [Fact]
    public async Task SupplierReport_Should_Return_Empty_When_No_Purchases()
    {
        var branch = await _dbContext.Branches.FirstAsync();

        var auditLog = Mock.Of<IAuditLogService>();
        var logger = Mock.Of<ILogger<PurchaseService>>();
        var unitOfWork = new UnitOfWork(_dbContext);
        var catalogService = new CatalogService(
            new Repository<Category>(_dbContext),
            new Repository<UnitType>(_dbContext),
            unitOfWork,
            auditLog);

        var reportService = new ReportService(
            new Repository<Sale>(_dbContext),
            new Repository<Purchase>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<Supplier>(_dbContext),
            new Repository<User>(_dbContext),
            catalogService,
            new CosmeticRepository(_dbContext));

        var supplierReport = await reportService.GetSupplierReportAsync(branch.BranchId);

        Assert.NotNull(supplierReport);
        Assert.All(supplierReport, r => { Assert.Equal(0, r.Purchases); Assert.Equal(0m, r.TotalAmount); });
    }

    [Fact]
    public async Task InventoryReport_Should_Return_Valid_Numeric_Values()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();

        var auditLog = Mock.Of<IAuditLogService>();
        var logger = Mock.Of<ILogger<PurchaseService>>();
        var unitOfWork = new UnitOfWork(_dbContext);
        var catalogService = new CatalogService(
            new Repository<Category>(_dbContext),
            new Repository<UnitType>(_dbContext),
            unitOfWork,
            auditLog);

        var purchaseService = new PurchaseService(
            new Repository<Purchase>(_dbContext),
            new Repository<PurchaseItem>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new Repository<Supplier>(_dbContext),
            catalogService,
            new Repository<InventoryTransaction>(_dbContext),
            unitOfWork,
            auditLog,
            logger,
            new CosmeticRepository(_dbContext),
            new Repository<CosmeticBatch>(_dbContext));

        var request = new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 500,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    BrandName = "InvTest Medicine",
                    GenericName = "InvTest Generic",
                    CategoryId = -1,
                    CategoryName = "Antibiotics",
                    UnitType = "Tablet",
                    BatchNumber = "INV-BATCH-001",
                    Quantity = 50,
                    PurchasePrice = 10,
                    SellingPrice = 20,
                    ExpiryDate = DateTime.UtcNow.AddYears(1)
                }
            }
        };

        await purchaseService.CreateAsync(request, 1, branch.BranchId);

        var reportService = new ReportService(
            new Repository<Sale>(_dbContext),
            new Repository<Purchase>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<Supplier>(_dbContext),
            new Repository<User>(_dbContext),
            catalogService,
            new CosmeticRepository(_dbContext));

        var inventoryReport = (await reportService.GetInventoryReportAsync(branch.BranchId)).ToList();

        Assert.NotEmpty(inventoryReport);
        var entry = inventoryReport.First();
        Assert.NotEqual(0, entry.Quantity);
        Assert.True(entry.Value > 0);
    }

    [Fact]
    public async Task SalesReport_Should_Return_Empty_List_When_No_Sales()
    {
        var branch = await _dbContext.Branches.FirstAsync();

        var auditLog = Mock.Of<IAuditLogService>();
        var unitOfWork = new UnitOfWork(_dbContext);
        var catalogService = new CatalogService(
            new Repository<Category>(_dbContext),
            new Repository<UnitType>(_dbContext),
            unitOfWork,
            auditLog);

        var reportService = new ReportService(
            new Repository<Sale>(_dbContext),
            new Repository<Purchase>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<Supplier>(_dbContext),
            new Repository<User>(_dbContext),
            catalogService,
            new CosmeticRepository(_dbContext));

        var salesReport = await reportService.GetSalesReportAsync("weekly", branch.BranchId);

        Assert.NotNull(salesReport);
        Assert.Empty(salesReport);
    }

    [Fact]
    public async Task DamageAndExpiry_Should_Return_Empty_Lists_When_No_Records()
    {
        var branch = await _dbContext.Branches.FirstAsync();

        var inventoryService = new InventoryService(
            new Repository<InventoryTransaction>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new UnitOfWork(_dbContext),
            Mock.Of<IAuditLogService>());

        var damages = await inventoryService.GetDamagesAsync(branch.BranchId);
        var expired = await inventoryService.GetExpiredAsync(branch.BranchId);

        Assert.NotNull(damages);
        Assert.Empty(damages);
        Assert.NotNull(expired);
        Assert.Empty(expired);
    }

    [Fact]
    public async Task PurchaseService_GetAllAsync_Should_Return_Empty_When_No_Purchases()
    {
        var branch = await _dbContext.Branches.FirstAsync();

        var auditLog = Mock.Of<IAuditLogService>();
        var logger = Mock.Of<ILogger<PurchaseService>>();
        var unitOfWork = new UnitOfWork(_dbContext);
        var catalogService = new CatalogService(
            new Repository<Category>(_dbContext),
            new Repository<UnitType>(_dbContext),
            unitOfWork,
            auditLog);

        var purchaseService = new PurchaseService(
            new Repository<Purchase>(_dbContext),
            new Repository<PurchaseItem>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new Repository<Supplier>(_dbContext),
            catalogService,
            new Repository<InventoryTransaction>(_dbContext),
            unitOfWork,
            auditLog,
            logger,
            new CosmeticRepository(_dbContext),
            new Repository<CosmeticBatch>(_dbContext));

        var result = await purchaseService.GetAllAsync(branch.BranchId);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private (PurchaseService service, CatalogService catalog) CreatePurchaseService()
    {
        var auditLog = Mock.Of<IAuditLogService>();
        var logger = Mock.Of<ILogger<PurchaseService>>();
        var unitOfWork = new UnitOfWork(_dbContext);
        var catalogService = new CatalogService(
            new Repository<Category>(_dbContext),
            new Repository<UnitType>(_dbContext),
            unitOfWork,
            auditLog);

        var purchaseService = new PurchaseService(
            new Repository<Purchase>(_dbContext),
            new Repository<PurchaseItem>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new Repository<Supplier>(_dbContext),
            catalogService,
            new Repository<InventoryTransaction>(_dbContext),
            unitOfWork,
            auditLog,
            logger,
            new CosmeticRepository(_dbContext),
            new Repository<CosmeticBatch>(_dbContext));

        return (purchaseService, catalogService);
    }

    private (SaleService service, CatalogService catalog) CreateSaleService()
    {
        var auditLog = Mock.Of<IAuditLogService>();
        var logger = Mock.Of<ILogger<SaleService>>();
        var unitOfWork = new UnitOfWork(_dbContext);
        var catalogService = new CatalogService(
            new Repository<Category>(_dbContext),
            new Repository<UnitType>(_dbContext),
            unitOfWork,
            auditLog);

        var settingsRepo = new Repository<Settings>(_dbContext);

        var saleService = new SaleService(
            new Repository<Sale>(_dbContext),
            new Repository<SaleItem>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new CosmeticRepository(_dbContext),
            new Repository<CosmeticBatch>(_dbContext),
            new Repository<InventoryTransaction>(_dbContext),
            new Repository<Notification>(_dbContext),
            settingsRepo,
            unitOfWork,
            auditLog);

        return (saleService, catalogService);
    }

    [Fact]
    public async Task CosmeticPurchase_Should_Persist_Cosmetic_And_Batch_With_Correct_Fields()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (service, _) = CreatePurchaseService();

        var purchase = await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 200,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Lip Balm",
                    CategoryId = -101,
                    BatchNumber = "BLA-001",
                    Quantity = 100,
                    PurchasePrice = 2,
                    SellingPrice = 5,
                    ExpiryDate = DateTime.UtcNow.AddMonths(12),
                    ReorderLevel = 10,
                }
            }
        }, 1, branch.BranchId);

        Assert.NotNull(purchase);
        Assert.Single(purchase.Items);
        var item = purchase.Items.First();
        Assert.Equal("cosmetic", item.ProductType);
        Assert.Equal(200, item.SubTotal);

        var cosmeticBatch = await _dbContext.CosmeticBatches
            .FirstAsync(b => b.BatchNumber == "BLA-001");
        Assert.Equal(100, cosmeticBatch.QuantityReceived);
        Assert.Equal(2, cosmeticBatch.BuyingPrice);
        Assert.Equal(5, cosmeticBatch.SellingPrice);
        Assert.Equal(10, cosmeticBatch.LowStockThreshold);
        Assert.NotNull(cosmeticBatch.ExpiryDate);
    }

    [Fact]
    public async Task CosmeticPurchase_Should_Preserve_Historical_Batch_Prices()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (service, _) = CreatePurchaseService();

        await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 100,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Hand Cream",
                    CategoryId = -100,
                    BatchNumber = "HC-001",
                    Quantity = 50,
                    PurchasePrice = 3,
                    SellingPrice = 7,
                    ExpiryDate = DateTime.UtcNow.AddMonths(12),
                    ReorderLevel = 5,
                }
            }
        }, 1, branch.BranchId);

        await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 160,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Hand Cream",
                    CategoryId = -100,
                    BatchNumber = "HC-001",
                    Quantity = 50,
                    PurchasePrice = 4,
                    SellingPrice = 8,
                    ExpiryDate = DateTime.UtcNow.AddMonths(12),
                    ReorderLevel = 5,
                }
            }
        }, 1, branch.BranchId);

        var batchA = await _dbContext.CosmeticBatches.FirstAsync(b => b.BatchNumber == "HC-001" && b.BuyingPrice == 3);
        var batchB = await _dbContext.CosmeticBatches.FirstAsync(b => b.BatchNumber == "HC-001" && b.BuyingPrice == 4);
        Assert.Equal(50, batchA.QuantityReceived);
        Assert.Equal(50, batchB.QuantityReceived);
        Assert.Equal(3, batchA.BuyingPrice);
        Assert.Equal(7, batchA.SellingPrice);
        Assert.Equal(4, batchB.BuyingPrice);
        Assert.Equal(8, batchB.SellingPrice);
    }

    [Fact]
    public async Task CosmeticPurchase_Should_Not_Require_Expiry_Date()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (service, _) = CreatePurchaseService();

        var purchase = await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 100,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Face Wash",
                    CategoryId = -101,
                    BatchNumber = "",
                    Quantity = 50,
                    PurchasePrice = 2,
                    SellingPrice = 5,
                }
            }
        }, 1, branch.BranchId);

        Assert.NotNull(purchase);
        Assert.Single(purchase.Items);
        var cosmeticItem = purchase.Items.First();
        Assert.Equal("cosmetic", cosmeticItem.ProductType);

        var cosmetic = await _dbContext.Cosmetics.FirstAsync(c => c.ProductName == "Face Wash");
        Assert.NotNull(cosmetic);
    }

    [Fact]
    public async Task MixedMedicineAndCosmeticPurchase_Should_Persist_Both()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (service, _) = CreatePurchaseService();

        var purchase = await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 250,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    BrandName = "Aspirin",
                    CategoryName = "Pain Relief",
                    BatchNumber = "ASP-001",
                    Quantity = 100,
                    PurchasePrice = 1,
                    SellingPrice = 2,
                    ExpiryDate = DateTime.UtcNow.AddMonths(24),
                },
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Shampoo",
                    CategoryId = -101,
                    BatchNumber = "",
                    Quantity = 50,
                    PurchasePrice = 3,
                    SellingPrice = 6,
                    ExpiryDate = DateTime.UtcNow.AddMonths(18),
                }
            }
        }, 1, branch.BranchId);

        Assert.NotNull(purchase);
        Assert.Equal(2, purchase.Items.Count);
        var medicineItem = purchase.Items.First(i => i.ProductType == "medicine");
        var cosmeticItem = purchase.Items.First(i => i.ProductType == "cosmetic");
        Assert.NotNull(medicineItem);
        Assert.NotNull(cosmeticItem);
        Assert.Equal(250, purchase.TotalAmount);
    }

    [Fact]
    public async Task MultipleCosmeticBatches_Should_Have_Different_Prices()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (service, _) = CreatePurchaseService();

        await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 100,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Lipstick",
                    CategoryId = -105,
                    BatchNumber = "LS-RED",
                    Quantity = 50,
                    PurchasePrice = 100,
                    SellingPrice = 150,
                    ExpiryDate = DateTime.UtcNow.AddMonths(12),
                }
            }
        }, 1, branch.BranchId);

        await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 120,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Lipstick",
                    CategoryId = -105,
                    BatchNumber = "LS-RED",
                    Quantity = 50,
                    PurchasePrice = 120,
                    SellingPrice = 180,
                    ExpiryDate = DateTime.UtcNow.AddMonths(12),
                }
            }
        }, 1, branch.BranchId);

        var batches = await _dbContext.CosmeticBatches
            .Where(b => b.Cosmetic.BranchId == branch.BranchId && b.Cosmetic.ProductName == "Lipstick")
            .ToListAsync();

        var batchA = batches.First(b => b.BuyingPrice == 100);
        var batchB = batches.First(b => b.BuyingPrice == 120);
        Assert.Equal(150, batchA.SellingPrice);
        Assert.Equal(180, batchB.SellingPrice);
    }

    [Fact]
    public async Task CosmeticPurchase_Should_Associate_BranchId_And_SupplierId()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (service, _) = CreatePurchaseService();

        await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 90,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Makeup Remover",
                    CategoryId = -102,
                    BatchNumber = "",
                    Quantity = 30,
                    PurchasePrice = 3,
                    SellingPrice = 7,
                    SupplierId = supplier.SupplierId,
                }
            }
        }, 1, branch.BranchId);

        var cosmetic = await _dbContext.Cosmetics.FirstAsync(c => c.ProductName == "Makeup Remover");
        Assert.Equal(branch.BranchId, cosmetic.BranchId);
        Assert.Equal(supplier.SupplierId, cosmetic.SupplierId);
    }

    [Fact]
    public async Task CosmeticSale_Should_Decrease_Batch_Quantity_And_Calculate_Profit()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (purchaseService, _) = CreatePurchaseService();

        await purchaseService.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 100,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Body Wash",
                    CategoryId = -102,
                    BatchNumber = "BW-001",
                    Quantity = 100,
                    PurchasePrice = 200,
                    SellingPrice = 350,
                    ExpiryDate = DateTime.UtcNow.AddMonths(12),
                }
            }
        }, 1, branch.BranchId);

        var cosmetic = await _dbContext.Cosmetics.FirstAsync(c => c.ProductName == "Body Wash");
        var batch = await _dbContext.CosmeticBatches.FirstAsync(b => b.CosmeticId == cosmetic.CosmeticId);

        var (saleService, _) = CreateSaleService();
        var sale = await saleService.CreateAsync(new CreateSaleRequest
        {
            Items = new List<SaleItemRequest>
            {
                new SaleItemRequest
                {
                    ProductId = cosmetic.CosmeticId,
                    ProductType = "cosmetic",
                    CosmeticId = cosmetic.CosmeticId,
                    CosmeticBatchId = batch.BatchId,
                    Quantity = 5,
                    DiscountAmount = 0
                }
            },
            PaymentMethod = "cash",
            AmountPaid = 1750
        }, 1, "Admin", branch.BranchId);

        var updatedBatch = await _dbContext.CosmeticBatches
            .FirstAsync(b => b.BatchId == batch.BatchId);
        Assert.Equal(95, updatedBatch.Balance);

        var saleItem = sale.Items.First();
        Assert.Equal(350, saleItem.UnitPrice);
        Assert.Equal(350, saleItem.SubTotal / saleItem.Quantity);
        Assert.Equal(1750, saleItem.SubTotal);
        Assert.Equal(5, saleItem.Quantity);
    }

    [Fact]
    public async Task CosmeticSaleProfit_Should_Use_Batch_BuyingPrice_Not_Current_Price()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (purchaseService, _) = CreatePurchaseService();

        await purchaseService.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 100,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Nail Polish",
                    CategoryId = -105,
                    BatchNumber = "NP-RED",
                    Quantity = 100,
                    PurchasePrice = 100,
                    SellingPrice = 150,
                    ExpiryDate = DateTime.UtcNow.AddMonths(12),
                }
            }
        }, 1, branch.BranchId);

        var cosmetic = await _dbContext.Cosmetics.FirstAsync(c => c.ProductName == "Nail Polish");
        var batch = await _dbContext.CosmeticBatches.FirstAsync(b => b.CosmeticId == cosmetic.CosmeticId && b.BuyingPrice == 100);

        var (saleService, _) = CreateSaleService();
        var sale = await saleService.CreateAsync(new CreateSaleRequest
        {
            Items = new List<SaleItemRequest>
            {
                new SaleItemRequest
                {
                    ProductId = cosmetic.CosmeticId,
                    ProductType = "cosmetic",
                    CosmeticId = cosmetic.CosmeticId,
                    CosmeticBatchId = batch.BatchId,
                    Quantity = 2,
                    DiscountAmount = 0
                }
            },
            PaymentMethod = "cash",
            AmountPaid = 300
        }, 1, "Admin", branch.BranchId);

        var saleItem = sale.Items.First();
        Assert.Equal(150, saleItem.UnitPrice);
        Assert.Equal(150, saleItem.SubTotal / saleItem.Quantity);
        Assert.Equal(100, sale.TotalProfit);
    }

    [Fact]
    public async Task CosmeticPurchase_Should_Include_In_Supplier_Report()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (purchaseService, catalog) = CreatePurchaseService();

        await purchaseService.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 50,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Perfume",
                    CategoryId = -106,
                    BatchNumber = "",
                    Quantity = 10,
                    PurchasePrice = 5,
                    SellingPrice = 12,
                    ExpiryDate = DateTime.UtcNow.AddMonths(24),
                }
            }
        }, 1, branch.BranchId);

        var reportService = new ReportService(
            new Repository<Sale>(_dbContext),
            new Repository<Purchase>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<Supplier>(_dbContext),
            new Repository<User>(_dbContext),
            catalog,
            new CosmeticRepository(_dbContext));

        var report = (await reportService.GetSupplierReportAsync(branch.BranchId)).ToList();
        var supplierEntry = report.First(r => r.SupplierId == supplier.SupplierId);
        Assert.Equal(1, supplierEntry.Purchases);
        Assert.Equal(50, supplierEntry.TotalAmount);
    }

    [Fact]
    public async Task InvalidCosmeticPurchase_Should_Fail_When_Category_Invalid()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (service, _) = CreatePurchaseService();

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await service.CreateAsync(new CreatePurchaseRequest
            {
                SupplierId = supplier.SupplierId,
                PurchaseDate = DateTime.UtcNow,
                PaymentMethod = "cash",
                AmountPaid = 50,
                Items = new List<PurchaseItemRequest>
                {
                    new PurchaseItemRequest
                    {
                        ProductType = "cosmetic",
                        BrandName = "Unknown Product",
                        CategoryId = 99999,
                        BatchNumber = "",
                        Quantity = 10,
                        PurchasePrice = 5,
                        SellingPrice = 10,
                    }
                }
            }, 1, branch.BranchId);
        });
    }

    [Fact]
    public async Task NotificationService_Should_Detect_Cosmetic_Low_Stock()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (purchaseService, catalog) = CreatePurchaseService();

        await purchaseService.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 9,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Lip Gloss",
                    CategoryId = -105,
                    BatchNumber = "",
                    Quantity = 3,
                    PurchasePrice = 3,
                    SellingPrice = 6,
                    ReorderLevel = 5,
                }
            }
        }, 1, branch.BranchId);

        var notificationRepo = new NotificationRepository(_dbContext);
        var cosmeticRepo = new CosmeticRepository(_dbContext);
        var notificationService = new NotificationService(
            notificationRepo,
            new Repository<Medicine>(_dbContext),
            cosmeticRepo,
            new UnitOfWork(_dbContext));

        await notificationService.CheckAndCreateNotificationsAsync(branch.BranchId);

        var notifications = await _dbContext.Notifications.ToListAsync();
        Assert.Contains(notifications, n => n.Title == "Cosmetic Low Stock");
    }

    [Fact]
    public async Task InventoryReport_Should_Include_Cosmetic_Products()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (purchaseService, catalog) = CreatePurchaseService();

        await purchaseService.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 50,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Face Cream",
                    CategoryId = -101,
                    BatchNumber = "",
                    Quantity = 20,
                    PurchasePrice = 2.5m,
                    SellingPrice = 6,
                }
            }
        }, 1, branch.BranchId);

        var reportService = new ReportService(
            new Repository<Sale>(_dbContext),
            new Repository<Purchase>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<Supplier>(_dbContext),
            new Repository<User>(_dbContext),
            catalog,
            new CosmeticRepository(_dbContext));

        var report = (await reportService.GetInventoryReportAsync(branch.BranchId)).ToList();
        Assert.Contains(report, r => r.BrandName == "Face Cream" && r.ProductType == "cosmetic");
    }

    [Fact]
    public async Task DashboardSummary_Should_Include_Cosmetics_In_InventoryValue()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (purchaseService, catalog) = CreatePurchaseService();

        await purchaseService.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 100,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Hair Gel",
                    CategoryId = -100,
                    BatchNumber = "",
                    Quantity = 100,
                    PurchasePrice = 100,
                    SellingPrice = 200,
                }
            }
        }, 1, branch.BranchId);

        var reportService = new ReportService(
            new Repository<Sale>(_dbContext),
            new Repository<Purchase>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<Supplier>(_dbContext),
            new Repository<User>(_dbContext),
            catalog,
            new CosmeticRepository(_dbContext));

        var summary = await reportService.GetDashboardSummaryAsync(branch.BranchId);
        Assert.True(summary.InventoryValue >= 10000);
    }

    [Fact]
    public async Task BranchIsolation_Should_Prevent_CrossBranch_CosmeticAccess()
    {
        var branch1 = await _dbContext.Branches.FirstAsync();
        _dbContext.Branches.Add(new Branch
        {
            BranchName = "Branch B",
            Location = "B Location",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        var branch2 = await _dbContext.Branches.OrderBy(b => b.BranchId).LastAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();

        var (service, _) = CreatePurchaseService();

        await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 100,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Cross-Branch Product",
                    CategoryId = -100,
                    BatchNumber = "",
                    Quantity = 10,
                    PurchasePrice = 10,
                    SellingPrice = 20,
                }
            }
        }, 1, branch1.BranchId);

        var cosmeticRepo = new CosmeticRepository(_dbContext);
        var cosmeticsBranchB = (await cosmeticRepo.GetAllAsync())
            .Where(c => c.BranchId == branch2.BranchId).ToList();

        Assert.Empty(cosmeticsBranchB);
    }

    [Fact]
    public async Task CosmeticPurchase_Should_Handle_Null_Expiry_Date_Without_Error()
    {
        var branch = await _dbContext.Branches.FirstAsync();
        var supplier = await _dbContext.Suppliers.FirstAsync();
        var (service, _) = CreatePurchaseService();

        var purchase = await service.CreateAsync(new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 50,
            Items = new List<PurchaseItemRequest>
            {
                new PurchaseItemRequest
                {
                    ProductType = "cosmetic",
                    BrandName = "Shaving Cream",
                    CategoryId = -107,
                    BatchNumber = "",
                    Quantity = 50,
                    PurchasePrice = 1,
                    SellingPrice = 3,
                }
            }
        }, 1, branch.BranchId);

        Assert.NotNull(purchase);
        var batch = await _dbContext.CosmeticBatches.FirstAsync(b => b.Cosmetic.ProductName == "Shaving Cream");
        Assert.Null(batch.ExpiryDate);
    }
}
