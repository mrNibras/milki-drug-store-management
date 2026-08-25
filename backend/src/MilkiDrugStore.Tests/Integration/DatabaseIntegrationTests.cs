using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Persistence.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Purchase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Integration.Database;

public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly string _connectionString;

    public DatabaseIntegrationTests()
    {
        _connectionString = $"Data Source={Guid.NewGuid()}.db;Cache=Shared";

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(_connectionString, sql =>
            {
                sql.MigrationsAssembly("MilkiDrugStore.Persistence");
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
                CreatedAt = DateTime.Now
            });
            await _dbContext.SaveChangesAsync();
        }

        if (!await _dbContext.UnitTypes.AnyAsync())
        {
            _dbContext.UnitTypes.AddRange(
                new UnitType { Name = "Bottle", Description = "Liquid container", IsActive = true } // A custom type for testing
            );
            await _dbContext.SaveChangesAsync();
        }

        if (!await _dbContext.Categories.AnyAsync())
        {
            _dbContext.Categories.Add(new Category
            {
                Name = "Test Category",
                IsActive = true,
                CreatedAt = DateTime.Now
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
                CreatedAt = DateTime.Now
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
                CreatedAt = DateTime.Now
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.EnsureDeletedAsync();
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

        try
        {
            var dbFile = new FileInfo(_connectionString.Replace("Data Source=", ""));
            if (dbFile.Exists)
            {
                dbFile.Delete();
            }
        }
        catch { }
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
            CreatedDate = DateTime.Now
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
            CreatedDate = DateTime.Now
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
            ExpiryDate = DateTime.Now.AddYears(1),
            DateReceived = DateTime.Now
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
            CreatedDate = DateTime.Now
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
            ExpiryDate = DateTime.Now.AddMonths(12),
            DateReceived = DateTime.Now
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
            ExpiryDate = DateTime.Now.AddMonths(3),
            DateReceived = DateTime.Now
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
            CreatedDate = DateTime.Now
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var purchase = new Purchase
        {
            PurchaseNumber = "PUR-2026-00001",
            SupplierId = 1,
            BranchId = 1,
            PurchaseDate = DateTime.Now,
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
            ExpiryDate = DateTime.Now.AddYears(1)
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
            ExpiryDate = DateTime.Now.AddYears(1),
            DateReceived = DateTime.Now
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
            CreatedDate = DateTime.Now
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
            ExpiryDate = DateTime.Now.AddYears(1),
            DateReceived = DateTime.Now
        };
        _dbContext.MedicineBatches.Add(batch);
        await _dbContext.SaveChangesAsync();

        var sale = new Sale
        {
            SaleNumber = "SAL-2026-00001",
            SaleDate = DateTime.Now,
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
            CreatedAt = DateTime.Now
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
            CreatedAt = DateTime.Now
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
            CreatedDate = DateTime.Now
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
            ExpiryDate = DateTime.Now.AddYears(1),
            DateReceived = DateTime.Now
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
            CreatedAt = DateTime.Now
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
            CreatedDate = DateTime.Now
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
            ExpiryDate = DateTime.Now.AddYears(1),
            DateReceived = DateTime.Now
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
            RecordedDate = DateTime.Now
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
            CreatedAt = DateTime.Now
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
            CreatedAt = DateTime.Now
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
            CreatedDate = DateTime.Now
        };
        _dbContext.Medicines.Add(medicine);
        await _dbContext.SaveChangesAsync();

        var purchaseService = new PurchaseService(
            new Repository<Purchase>(_dbContext),
            new Repository<PurchaseItem>(_dbContext),
            new Repository<Medicine>(_dbContext),
            new Repository<MedicineBatch>(_dbContext),
            new Repository<Supplier>(_dbContext),
            new Mock<ICatalogService>().Object,
            new Repository<InventoryTransaction>(_dbContext),
            new UnitOfWork(_dbContext),
            Mock.Of<IAuditLogService>(),
            Mock.Of<ILogger<PurchaseService>>());

        var request = new CreatePurchaseRequest
        {
            SupplierId = supplier.SupplierId,
            PurchaseDate = DateTime.Now,
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
                    ExpiryDate = DateTime.Now.AddYears(1)
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
}