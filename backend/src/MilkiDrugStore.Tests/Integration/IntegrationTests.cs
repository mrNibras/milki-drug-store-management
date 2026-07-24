using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Infrastructure.Services;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

public class IntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;

    public IntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: $"MilkiDrugStore_Test_{Guid.NewGuid()}"));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAuthService, MilkiDrugStore.Application.Services.AuthService>();
        services.AddScoped<IJwtTokenService, MilkiDrugStore.Infrastructure.Services.JwtTokenService>();
        services.AddScoped<IEmailService, MilkiDrugStore.Infrastructure.Services.EmailService>();
        services.AddScoped<IAuditLogService, MilkiDrugStore.Application.Services.AuditLogService>();
        services.AddScoped<IBranchService, MilkiDrugStore.Application.Services.BranchService>();
        services.AddScoped<ICosmeticService, MilkiDrugStore.Application.Services.CosmeticService>();
        services.AddScoped<ICosmeticCategoryService, MilkiDrugStore.Application.Services.CosmeticCategoryService>();
        services.AddScoped<IBackupService, MilkiDrugStore.Application.Services.BackupService>();

        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
    }

    [Fact]
    public async Task Database_Should_Initialize_With_Sample_Data()
    {
        _dbContext.Database.EnsureCreated();

        var role = new Role { Name = "Admin" };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        Assert.True(await _dbContext.Roles.AnyAsync());
    }

    [Fact]
    public async Task CosmeticCategory_Should_Be_Persisted()
    {
        _dbContext.Database.EnsureCreated();

        var category = new CosmeticCategory
        {
            Name = "Test Category",
            Description = "Test Description",
            IsActive = true
        };

        _dbContext.CosmeticCategories.Add(category);
        await _dbContext.SaveChangesAsync();

        var saved = await _dbContext.CosmeticCategories.FirstOrDefaultAsync(c => c.Name == "Test Category");
        Assert.NotNull(saved);
        Assert.Equal("Test Description", saved.Description);
    }

    [Fact]
    public async Task Cosmetic_Should_Be_Persisted_With_Batch()
    {
        _dbContext.Database.EnsureCreated();

        var cosmetic = new Cosmetic
        {
            ProductName = "Test Cream",
            Description = "Test Description",
            CosmeticCategoryId = 1,
            UnitTypeId = 1,
            Price = 99.99m,
            IsActive = true
        };

        _dbContext.Cosmetics.Add(cosmetic);
        await _dbContext.SaveChangesAsync();

        var batch = new CosmeticBatch
        {
            CosmeticId = cosmetic.CosmeticId,
            BatchNumber = "BATCH-001",
            QuantityReceived = 100,
            ExpiryDate = DateTime.Now.AddYears(1)
        };

        _dbContext.CosmeticBatches.Add(batch);
        await _dbContext.SaveChangesAsync();

        var loadedBatch = await _dbContext.CosmeticBatches.FirstAsync();
        Assert.Equal(100, loadedBatch.Balance);
    }

    public Task InitializeAsync() => Task.CompletedTask;

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
    }
}
