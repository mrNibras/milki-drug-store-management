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

public class InventoryServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly InventoryService _sut;

    public InventoryServiceTests()
    {
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Inventory_{Guid.NewGuid()}")
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

    private async Task<(CosmeticBatch Batch, Branch Branch, Cosmetic Cosmetic)> SeedCosmeticBatchAsync(
        int quantityReceived = 100, DateTime? expiryDate = null)
    {
        var branch = new Branch { BranchName = "Main", IsActive = true };
        var cosmetic = new Cosmetic
        {
            ProductName = "Moisturizer",
            CategoryId = 1,
            UnitTypeId = 1,
            Price = 300m,
            IsActive = true
        };
        var batch = new CosmeticBatch
        {
            CosmeticId = 0,
            Branch = branch,
            BatchNumber = "COS-001",
            QuantityReceived = quantityReceived,
            BuyingPrice = 120m,
            SellingPrice = 300m,
            ExpiryDate = expiryDate ?? DateTime.UtcNow.AddYears(1)
        };

        _db.Cosmetics.Add(cosmetic);
        _db.Branches.Add(branch);
        _db.CosmeticBatches.Add(batch);
        await _db.SaveChangesAsync();

        batch.CosmeticId = cosmetic.CosmeticId;
        await _db.SaveChangesAsync();

        return (batch, branch, cosmetic);
    }

    // ---------- Damage: medicine ----------

    [Fact]
    public async Task RecordDamage_Medicine_ReducesStockAndPersistsHistory()
    {
        var (batch, _, _) = await SeedMedicineBatchAsync(quantityReceived: 100);

        await _sut.RecordDamageAsync(batch.BatchId, null, 10, "Broken glass", recordedBy: 7);

        _db.ChangeTracker.Clear();
        var reloaded = await _db.MedicineBatches.SingleAsync();
        reloaded.RemainingQuantity.Should().Be(90);
        reloaded.QuantityDamaged.Should().Be(10);

        var damage = await _db.DamageRecords.SingleAsync();
        damage.BatchId.Should().Be(batch.BatchId);
        damage.CosmeticBatchId.Should().BeNull();
        damage.Quantity.Should().Be(10);
        damage.Reason.Should().Be("Broken glass");
        damage.RecordedBy.Should().Be(7);

        var transaction = await _db.InventoryTransactions.SingleAsync();
        transaction.BatchId.Should().Be(batch.BatchId);
        transaction.CosmeticBatchId.Should().BeNull();
        transaction.ProductId.Should().Be(reloaded.ProductId);
        transaction.TransactionType.Should().Be("Damage");
        transaction.Quantity.Should().Be(10);
        transaction.UnitPrice.Should().Be(50m);
        transaction.ReferenceType.Should().Be("DAMAGE");
    }

    // ---------- Damage: cosmetic ----------

    [Fact]
    public async Task RecordDamage_Cosmetic_ReducesBalanceAndPersistsHistory()
    {
        var (batch, _, _) = await SeedCosmeticBatchAsync(quantityReceived: 100);

        await _sut.RecordDamageAsync(null, batch.BatchId, 10, "Leaking cap", recordedBy: 7);

        _db.ChangeTracker.Clear();
        var reloaded = await _db.CosmeticBatches.SingleAsync();
        reloaded.Balance.Should().Be(90);
        reloaded.QuantityDamaged.Should().Be(10);

        var damage = await _db.DamageRecords.SingleAsync();
        damage.CosmeticBatchId.Should().Be(batch.BatchId);
        damage.CosmeticId.Should().Be(reloaded.CosmeticId);
        damage.BatchId.Should().BeNull();
        damage.Quantity.Should().Be(10);
        damage.Reason.Should().Be("Leaking cap");

        var transaction = await _db.InventoryTransactions.SingleAsync();
        transaction.CosmeticBatchId.Should().Be(batch.BatchId);
        transaction.CosmeticId.Should().Be(reloaded.CosmeticId);
        transaction.ProductId.Should().BeNull();
        transaction.Quantity.Should().Be(10);
        transaction.UnitPrice.Should().Be(120m);
    }

    // ---------- Damage validation ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RecordDamage_InvalidQuantity_IsRejected(int quantity)
    {
        var (batch, _, _) = await SeedMedicineBatchAsync();

        await FluentActions
            .Awaiting(() => _sut.RecordDamageAsync(batch.BatchId, null, quantity, "reason", 1))
            .Should().ThrowAsync<ArgumentException>();

        (await _db.DamageRecords.CountAsync()).Should().Be(0);
        (await _db.InventoryTransactions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RecordDamage_ExceedingStock_IsRejectedAndLeavesNoPartialWrite()
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
    public async Task RecordDamage_NoBatchIdentifier_IsRejected()
    {
        await FluentActions
            .Awaiting(() => _sut.RecordDamageAsync(null, null, 5, "reason", 1))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RecordDamage_BothBatchIdentifiers_IsRejected()
    {
        var (medicine, _, _) = await SeedMedicineBatchAsync();
        var (cosmetic, _, _) = await SeedCosmeticBatchAsync();

        await FluentActions
            .Awaiting(() => _sut.RecordDamageAsync(medicine.BatchId, cosmetic.BatchId, 5, "reason", 1))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RecordDamage_UnknownMedicineBatch_IsRejectedSafely()
    {
        await FluentActions
            .Awaiting(() => _sut.RecordDamageAsync(99999, null, 5, "reason", 1))
            .Should().ThrowAsync<ArgumentException>().WithMessage("*not found*");

        (await _db.DamageRecords.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RecordDamage_UnknownCosmeticBatch_IsRejectedSafely()
    {
        await FluentActions
            .Awaiting(() => _sut.RecordDamageAsync(null, 99999, 5, "reason", 1))
            .Should().ThrowAsync<ArgumentException>().WithMessage("*not found*");

        (await _db.DamageRecords.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RecordDamage_EmptyReason_IsRejected()
    {
        var (batch, _, _) = await SeedMedicineBatchAsync();

        await FluentActions
            .Awaiting(() => _sut.RecordDamageAsync(batch.BatchId, null, 5, "   ", 1))
            .Should().ThrowAsync<ArgumentException>();
    }

    // ---------- Automatic expiry ----------

    [Fact]
    public async Task ProcessExpired_PastMedicineBatch_IsWrittenOffAutomatically()
    {
        var (batch, _, _) = await SeedMedicineBatchAsync(100, DateTime.UtcNow.AddDays(-1));

        var result = await _sut.ProcessExpiredInventoryAsync();

        result.MedicineBatchesProcessed.Should().Be(1);
        result.CosmeticBatchesProcessed.Should().Be(0);
        result.TotalUnitsExpired.Should().Be(100);

        _db.ChangeTracker.Clear();
        (await _db.MedicineBatches.SingleAsync()).RemainingQuantity.Should().Be(0);
        (await _db.MedicineBatches.SingleAsync()).QuantityExpired.Should().Be(100);

        var expired = await _db.ExpiredRecords.SingleAsync();
        expired.BatchId.Should().Be(batch.BatchId);
        expired.Quantity.Should().Be(100);

        var transaction = await _db.InventoryTransactions.SingleAsync();
        transaction.TransactionType.Should().Be("Expired");
        transaction.Quantity.Should().Be(100);
    }

    [Fact]
    public async Task ProcessExpired_PastCosmeticBatch_IsWrittenOffAutomatically()
    {
        var (batch, _, _) = await SeedCosmeticBatchAsync(100, DateTime.UtcNow.AddDays(-1));

        var result = await _sut.ProcessExpiredInventoryAsync();

        result.CosmeticBatchesProcessed.Should().Be(1);
        result.MedicineBatchesProcessed.Should().Be(0);
        result.TotalUnitsExpired.Should().Be(100);

        _db.ChangeTracker.Clear();
        (await _db.CosmeticBatches.SingleAsync()).Balance.Should().Be(0);

        var expired = await _db.ExpiredRecords.SingleAsync();
        expired.CosmeticBatchId.Should().Be(batch.BatchId);
        expired.BatchId.Should().BeNull();
        expired.Quantity.Should().Be(100);

        var transaction = await _db.InventoryTransactions.SingleAsync();
        transaction.CosmeticBatchId.Should().Be(batch.BatchId);
        transaction.TransactionType.Should().Be("Expired");
    }

    [Fact]
    public async Task ProcessExpired_FutureExpiry_IsNotProcessed()
    {
        await SeedMedicineBatchAsync(100, DateTime.UtcNow.AddYears(2));
        await SeedCosmeticBatchAsync(100, DateTime.UtcNow.AddYears(2));

        var result = await _sut.ProcessExpiredInventoryAsync();

        result.TotalUnitsExpired.Should().Be(0);
        (await _db.ExpiredRecords.CountAsync()).Should().Be(0);
        (await _db.InventoryTransactions.CountAsync()).Should().Be(0);
        (await _db.MedicineBatches.SingleAsync()).RemainingQuantity.Should().Be(100);
        (await _db.CosmeticBatches.SingleAsync()).Balance.Should().Be(100);
    }

    [Fact]
    public async Task ProcessExpired_RunTwice_DoesNotDuplicateExpiryRecords()
    {
        await SeedMedicineBatchAsync(100, DateTime.UtcNow.AddDays(-5));
        await SeedCosmeticBatchAsync(100, DateTime.UtcNow.AddDays(-5));

        var first = await _sut.ProcessExpiredInventoryAsync();
        var second = await _sut.ProcessExpiredInventoryAsync();

        first.TotalUnitsExpired.Should().Be(200);
        second.TotalUnitsExpired.Should().Be(0);
        second.MedicineBatchesProcessed.Should().Be(0);
        second.CosmeticBatchesProcessed.Should().Be(0);

        (await _db.ExpiredRecords.CountAsync()).Should().Be(2);
        (await _db.InventoryTransactions.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task ProcessExpired_OnlyWritesOffRemainingStock()
    {
        var (_, _, _) = await SeedMedicineBatchAsync(100, DateTime.UtcNow.AddDays(-1));
        var batch = await _db.MedicineBatches.SingleAsync();
        batch.QuantityIssued = 30;
        await _db.SaveChangesAsync();

        var result = await _sut.ProcessExpiredInventoryAsync();

        result.TotalUnitsExpired.Should().Be(70);
        _db.ChangeTracker.Clear();
        (await _db.MedicineBatches.SingleAsync()).RemainingQuantity.Should().Be(0);
        (await _db.ExpiredRecords.SingleAsync()).Quantity.Should().Be(70);
    }

    [Fact]
    public async Task ProcessExpired_AlreadyZeroStock_IsSkipped()
    {
        var (_, _, _) = await SeedMedicineBatchAsync(100, DateTime.UtcNow.AddDays(-1));
        var batch = await _db.MedicineBatches.SingleAsync();
        batch.QuantityExpired = 100;
        await _db.SaveChangesAsync();

        var result = await _sut.ProcessExpiredInventoryAsync();

        result.MedicineBatchesProcessed.Should().Be(0);
        (await _db.ExpiredRecords.CountAsync()).Should().Be(0);
    }

    // ---------- Reads ----------

    [Fact]
    public async Task GetDamages_DistinguishesMedicineAndCosmetic()
    {
        var (medicine, _, _) = await SeedMedicineBatchAsync();
        var (cosmetic, _, _) = await SeedCosmeticBatchAsync();

        await _sut.RecordDamageAsync(medicine.BatchId, null, 5, "medicine damage", 1);
        await _sut.RecordDamageAsync(null, cosmetic.BatchId, 7, "cosmetic damage", 1);

        var damages = (await _sut.GetDamagesAsync()).ToList();

        damages.Should().HaveCount(2);

        var medicineDamage = damages.Single(d => d.ProductType == "medicine");
        medicineDamage.BatchNumber.Should().Be("MED-001");
        medicineDamage.BrandName.Should().Be("Panadol");
        medicineDamage.CosmeticBatchId.Should().BeNull();
        medicineDamage.ProductId.Should().NotBeNull();

        var cosmeticDamage = damages.Single(d => d.ProductType == "cosmetic");
        cosmeticDamage.BatchNumber.Should().Be("COS-001");
        cosmeticDamage.BrandName.Should().Be("Moisturizer");
        cosmeticDamage.BatchId.Should().BeNull();
        cosmeticDamage.CosmeticId.Should().NotBeNull();
    }

    [Fact]
    public async Task GetExpired_IncludesAutomaticallyRecordedCosmeticExpiry()
    {
        var (_, _, _) = await SeedCosmeticBatchAsync(100, DateTime.UtcNow.AddDays(-2));
        await _sut.ProcessExpiredInventoryAsync();

        var expired = (await _sut.GetExpiredAsync()).ToList();

        expired.Should().HaveCount(1);
        expired[0].ProductType.Should().Be("cosmetic");
        expired[0].BatchNumber.Should().Be("COS-001");
        expired[0].BrandName.Should().Be("Moisturizer");
        expired[0].Quantity.Should().Be(100);
    }

    [Fact]
    public void BusinessClock_UsesEthiopiaOffset()
    {
        var local = BusinessClock.LocalNow;
        var utc = DateTime.UtcNow;

        Math.Abs((local - utc).TotalHours).Should().BeApproximately(3.0, 0.1);
    }
}
