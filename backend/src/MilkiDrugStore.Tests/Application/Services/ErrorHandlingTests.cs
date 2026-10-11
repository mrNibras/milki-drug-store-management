using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Common;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application.Services;

/// <summary>
/// Verifies that failed operations behave safely: they throw a clear,
/// typed exception and leave no partial or duplicate writes behind.
/// These tests use an isolated in-memory database, so no production
/// data is touched.
/// </summary>
public class ErrorHandlingTests
{
    private readonly AppDbContext _db;
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly InventoryService _sut;

    public ErrorHandlingTests()
    {
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Err_{Guid.NewGuid()}")
            .Options);
        _sut = new InventoryService(
            new Repository<InventoryTransaction>(_db),
            new Repository<MedicineBatch>(_db),
            new Repository<Medicine>(_db),
            new Repository<CosmeticBatch>(_db),
            new UnitOfWork(_db),
            _auditLog.Object);
    }

    public void Dispose() => _db.Dispose();

    private async Task<(MedicineBatch Batch, Branch Branch, Medicine Medicine)> SeedMedicineBatchAsync(
        int quantityReceived = 100, DateTime? expiryDate = null)
    {
        var branch = new Branch { BranchName = "Main", IsActive = true };
        var medicine = new Medicine
        {
            BrandName = "Panadol",
            GenericName = "Paracetamol",
            CategoryId = 1,
            UnitTypeId = 1,
            IsActive = true
        };
        var batch = new MedicineBatch
        {
            ProductId = 0,
            Branch = branch,
            BatchNumber = "MED-001",
            PurchasePrice = 50m,
            SellingPrice = 80m,
            QuantityReceived = quantityReceived,
            ExpiryDate = expiryDate ?? DateTime.UtcNow.AddYears(1)
        };

        _db.Medicines.Add(medicine);
        _db.Branches.Add(branch);
        _db.MedicineBatches.Add(batch);
        await _db.SaveChangesAsync();

        batch.ProductId = medicine.ProductId;
        await _db.SaveChangesAsync();

        return (batch, branch, medicine);
    }

    // ---------- Damage: failure leaves no partial write ----------

    [Fact]
    public async Task RecordDamage_UnknownBatch_ThrowsAndWritesNothing()
    {
        await FluentActions
            .Awaiting(() => _sut.RecordDamageAsync(99999, null, 5, "reason", 1))
            .Should().ThrowAsync<ArgumentException>().WithMessage("*not found*");

        (await _db.DamageRecords.CountAsync()).Should().Be(0);
        (await _db.InventoryTransactions.CountAsync()).Should().Be(0);
        (await _db.MedicineBatches.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RecordDamage_OverStock_ThrowsAndLeavesStockUntouched()
    {
        var (batch, _, _) = await SeedMedicineBatchAsync(quantityReceived: 100);

        await FluentActions
            .Awaiting(() => _sut.RecordDamageAsync(batch.BatchId, null, 101, "too many", 1))
            .Should().ThrowAsync<ArgumentException>();

        _db.ChangeTracker.Clear();
        (await _db.MedicineBatches.SingleAsync()).RemainingQuantity.Should().Be(100);
        (await _db.DamageRecords.CountAsync()).Should().Be(0);
        (await _db.InventoryTransactions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RecordDamage_InvalidQuantity_ThrowsAndPersistsNothing()
    {
        var (batch, _, _) = await SeedMedicineBatchAsync();

        await FluentActions
            .Awaiting(() => _sut.RecordDamageAsync(batch.BatchId, null, 0, "reason", 1))
            .Should().ThrowAsync<ArgumentException>();

        (await _db.DamageRecords.CountAsync()).Should().Be(0);
        (await _db.InventoryTransactions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RecordDamage_EmptyReason_Throws()
    {
        var (batch, _, _) = await SeedMedicineBatchAsync();

        await FluentActions
            .Awaiting(() => _sut.RecordDamageAsync(batch.BatchId, null, 5, "   ", 1))
            .Should().ThrowAsync<ArgumentException>();
    }

    // ---------- Expiry: idempotent and safe ----------

    [Fact]
    public async Task ProcessExpired_RunTwice_DoesNotCreateDuplicateRecords()
    {
        await SeedMedicineBatchAsync(100, DateTime.UtcNow.AddDays(-1));

        var first = await _sut.ProcessExpiredInventoryAsync();
        var second = await _sut.ProcessExpiredInventoryAsync();

        first.TotalUnitsExpired.Should().Be(100);
        second.TotalUnitsExpired.Should().Be(0);
        (await _db.ExpiredRecords.CountAsync()).Should().Be(1);
        (await _db.InventoryTransactions.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ProcessExpired_FutureBatch_LeftAlone()
    {
        await SeedMedicineBatchAsync(100, DateTime.UtcNow.AddYears(2));

        var result = await _sut.ProcessExpiredInventoryAsync();

        result.TotalUnitsExpired.Should().Be(0);
        _db.ChangeTracker.Clear();
        (await _db.MedicineBatches.SingleAsync()).RemainingQuantity.Should().Be(100);
        (await _db.ExpiredRecords.CountAsync()).Should().Be(0);
    }
}