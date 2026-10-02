using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Catalog;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Integration.Database;

/// <summary>
/// End-to-end proof that a cosmetic's category survives the real PostgreSQL stack:
/// Database -> EF Core -> CatalogService -> CosmeticService -> DTO.
///
/// Every read runs in a fresh DI scope, so the navigation/catalog lookup cannot be
/// satisfied by a shared change tracker.
/// </summary>
public class CosmeticCategoryIntegrationTests : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly ServiceProvider _serviceProvider;
    private readonly IAuditLogService _auditLog = Mock.Of<IAuditLogService>();

    private int _branchId;
    private int _unitTypeId;

    public CosmeticCategoryIntegrationTests(PostgreSqlFixture fixture)
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
        services.AddSingleton(_auditLog);
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task InitializeAsync()
    {
        await InDbWriteAsync(async db =>
        {
            await db.Database.MigrateAsync();

            var branch = await db.Branches.FirstOrDefaultAsync(b => b.BranchName == "Cosmetic Category Branch");
            if (branch == null)
            {
                branch = new Branch { BranchName = "Cosmetic Category Branch", Location = "Test", IsActive = true };
                db.Branches.Add(branch);
                await db.SaveChangesAsync();
            }
            _branchId = branch.BranchId;

            var unit = await db.UnitTypes.FirstOrDefaultAsync(u => u.Name == "Cosmetic Category Unit");
            if (unit == null)
            {
                unit = new UnitType { Name = "Cosmetic Category Unit", IsActive = true };
                db.UnitTypes.Add(unit);
                await db.SaveChangesAsync();
            }
            _unitTypeId = unit.UnitTypeId;
        });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>The scope must outlive the awaited work, otherwise the context is disposed early.</summary>
    private async Task InDbWriteAsync(Func<AppDbContext, Task> work)
    {
        using var scope = _serviceProvider.CreateScope();
        await work(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    private async Task<T> InDbQueryAsync<T>(Func<AppDbContext, Task<T>> work)
    {
        using var scope = _serviceProvider.CreateScope();
        return await work(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    private async Task<T> WithServiceAsync<T>(Func<IServiceProvider, Task<T>> work)
    {
        using var scope = _serviceProvider.CreateScope();
        return await work(scope.ServiceProvider);
    }

    private Task<int> SeedCosmeticAsync(string suffix, int categoryId)
        => InDbQueryAsync(async db =>
        {
            var cosmetic = new Cosmetic
            {
                ProductName = $"Cat Test {suffix}-{Guid.NewGuid():N}"[..40],
                Description = "Cosmetic category integration test",
                CategoryId = categoryId,
                UnitTypeId = _unitTypeId,
                Price = 100,
                IsActive = true,
                BranchId = _branchId,
                CreatedAt = DateTime.UtcNow
            };
            db.Cosmetics.Add(cosmetic);
            await db.SaveChangesAsync();
            return cosmetic.CosmeticId;
        });

    private Task<int> SeedMedicineAsync(string suffix, int categoryId)
        => InDbQueryAsync(async db =>
        {
            var medicine = new Medicine
            {
                ProductCode = $"CCAT{suffix}-{Guid.NewGuid():N}"[..26],
                BrandName = $"Cat Med {suffix}",
                GenericName = suffix,
                CategoryId = categoryId,
                UnitTypeId = _unitTypeId,
                SellingPrice = 50,
                ReorderLevel = 10,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            db.Medicines.Add(medicine);
            await db.SaveChangesAsync();
            return medicine.ProductId;
        });

    private async Task<T> FetchCosmeticAsync<T>(int cosmeticId, Func<MilkiDrugStore.Application.DTOs.Cosmetic.CosmeticResponse, T> project)
        => await WithServiceAsync(async sp =>
        {
            var cosmetic = await sp.GetRequiredService<ICosmeticService>().GetByIdAsync(cosmeticId);
            return project(cosmetic!);
        });

    // --- 1/2/3: category round-trips through the database ---------------

    [Theory]
    [InlineData(-100, "Hair Care")]
    [InlineData(-104, "Baby Care")]
    [InlineData(-105, "Makeup")]
    [InlineData(-106, "Fragrance")]
    public async Task BuiltInCosmeticCategory_IsReturnedByApiAfterFreshDbContext(int categoryId, string expected)
    {
        var cosmeticId = await SeedCosmeticAsync("builtin", categoryId);

        var name = await FetchCosmeticAsync(cosmeticId, c => c.CategoryName);

        Assert.Equal(expected, name);
    }

    [Fact]
    public async Task AllBuiltInCosmeticCategories_AreReturnedForOneCosmeticEach()
    {
        foreach (var item in CosmeticCatalog.Categories)
        {
            var id = await SeedCosmeticAsync($"cat{item.Id}", item.Id);
            var name = await FetchCosmeticAsync(id, c => c.CategoryName);
            Assert.Equal(item.Name, name);
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDistinctCategoriesForDifferentCosmetics()
    {
        await SeedCosmeticAsync("multi-a", -101); // Skin Care
        await SeedCosmeticAsync("multi-b", -105); // Makeup

        var names = await WithServiceAsync(async sp =>
        {
            var all = await sp.GetRequiredService<ICosmeticService>().GetAllAsync();
            return all.Where(c => c.ProductName.StartsWith("Cat Test multi-"))
                      .Select(c => c.CategoryName)
                      .ToHashSet();
        });

        Assert.Contains("Skin Care", names);
        Assert.Contains("Makeup", names);
    }

    // --- Custom (persisted) category still resolves --------------------

    [Fact]
    public async Task CustomCategory_IsReturnedForCosmetic()
    {
        var customCategoryId = await InDbQueryAsync(async db =>
        {
            var existing = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Cosmetic Custom Line");
            if (existing != null) return existing.CategoryId;
            var created = new Category { Name = "Cosmetic Custom Line", IsActive = true };
            db.Categories.Add(created);
            await db.SaveChangesAsync();
            return created.CategoryId;
        });

        var cosmeticId = await SeedCosmeticAsync("custom", customCategoryId);
        Assert.Equal("Cosmetic Custom Line", await FetchCosmeticAsync(cosmeticId, c => c.CategoryName));
    }

    // --- 6: unknown category does not crash ----------------------------

    [Fact]
    public async Task UnknownCategoryId_DoesNotCrashAndReturnsEmptyName()
    {
        var cosmeticId = await SeedCosmeticAsync("unknown", 999999);

        var name = await FetchCosmeticAsync(cosmeticId, c => c.CategoryName);

        Assert.Equal("", name);
    }

    // --- 7: medicine category regression -------------------------------

    [Theory]
    [InlineData(-1, "Antibiotics")]
    [InlineData(-10, "Vitamins and Minerals")]
    public async Task MedicineCategory_IsStillReturnedAfterCosmeticFix(int categoryId, string expected)
    {
        var productId = await SeedMedicineAsync("reg", categoryId);

        var name = await WithServiceAsync(async sp =>
        {
            var all = await sp.GetRequiredService<IMedicineService>().GetAllAsync();
            return all.Single(m => m.ProductId == productId).CategoryName;
        });

        Assert.Equal(expected, name);
    }
}