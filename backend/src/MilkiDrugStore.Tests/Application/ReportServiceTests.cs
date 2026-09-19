using Xunit;
using FluentAssertions;
using Moq;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;

namespace MilkiDrugStore.Tests.Application;

public class ReportServiceTests
{
    private readonly Mock<IRepository<Sale>> _saleRepo = new();
    private readonly Mock<IRepository<Purchase>> _purchaseRepo = new();
    private readonly Mock<IRepository<Medicine>> _medicineRepo = new();
    private readonly Mock<IRepository<Supplier>> _supplierRepo = new();
    private readonly Mock<IRepository<User>> _userRepo = new();
    private readonly Mock<ICatalogService> _catalog = new();

    private ReportService CreateService(IEnumerable<Medicine> medicines)
    {
        _medicineRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(medicines.ToList().AsQueryable());
        _catalog.Setup(c => c.GetCategoryNamesAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new Dictionary<int, string>());
        return new ReportService(
            _saleRepo.Object,
            _purchaseRepo.Object,
            _medicineRepo.Object,
            _supplierRepo.Object,
            _userRepo.Object,
            _catalog.Object);
    }

    private static MedicineBatch Batch(int batchId, int productId, int quantityReceived, decimal purchasePrice,
        int quantityIssued = 0, int quantityDamaged = 0, int quantityExpired = 0)
        => new()
        {
            BatchId = batchId,
            ProductId = productId,
            BatchNumber = $"B{batchId}",
            QuantityReceived = quantityReceived,
            QuantityIssued = quantityIssued,
            QuantityDamaged = quantityDamaged,
            QuantityExpired = quantityExpired,
            PurchasePrice = purchasePrice,
            SellingPrice = purchasePrice * 2
        };

    [Fact]
    public async Task GetInventoryReportAsync_WithNormalStock_CalculatesPurchaseCostValue()
    {
        var service = CreateService(new[]
        {
            new Medicine
            {
                ProductId = 1,
                BrandName = "Paracetamol",
                ReorderLevel = 10,
                Batches = { Batch(1, 1, 100, 20m) }
            }
        });

        var result = (await service.GetInventoryReportAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Quantity.Should().Be(100);
        result[0].Value.Should().Be(2000m);
        result[0].Status.Should().Be("In Stock");
    }

    [Fact]
    public async Task GetInventoryReportAsync_WithZeroQuantity_ReturnsZeroValue()
    {
        var service = CreateService(new[]
        {
            new Medicine
            {
                ProductId = 1,
                BrandName = "Aspirin",
                ReorderLevel = 10,
                Batches = { Batch(1, 1, 0, 20m) }
            }
        });

        var result = (await service.GetInventoryReportAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Quantity.Should().Be(0);
        result[0].Value.Should().Be(0m);
        result[0].Status.Should().Be("Out of Stock");
    }

    [Fact]
    public async Task GetInventoryReportAsync_WithNoBatchesOrZeroPrice_ReturnsValidZero()
    {
        var service = CreateService(new[]
        {
            new Medicine { ProductId = 1, BrandName = "NoBatches", ReorderLevel = 5 },
            new Medicine
            {
                ProductId = 2,
                BrandName = "ZeroPrice",
                ReorderLevel = 5,
                Batches = { Batch(1, 2, 25, 0m) }
            }
        });

        var result = (await service.GetInventoryReportAsync()).ToList();

        result.Should().HaveCount(2);
        var noBatches = result.Single(r => r.ProductId == 1);
        var zeroPrice = result.Single(r => r.ProductId == 2);

        noBatches.Quantity.Should().Be(0);
        noBatches.Value.Should().Be(0m);
        noBatches.Status.Should().Be("Out of Stock");

        zeroPrice.Quantity.Should().Be(25);
        zeroPrice.Value.Should().Be(0m);
        zeroPrice.Status.Should().Be("In Stock");
    }

    [Fact]
    public async Task GetInventoryReportAsync_WithMultipleBatches_SumsPerBatchValue()
    {
        var service = CreateService(new[]
        {
            new Medicine
            {
                ProductId = 1,
                BrandName = "Amoxicillin",
                ReorderLevel = 10,
                Batches =
                {
                    Batch(1, 1, 50, 20m),
                    Batch(2, 1, 30, 25m)
                }
            }
        });

        var result = (await service.GetInventoryReportAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Quantity.Should().Be(50 + 30);
        result[0].Value.Should().Be((50m * 20m) + (30m * 25m));
        result[0].Value.Should().Be(1750m);
    }

    [Fact]
    public async Task GetInventoryReportAsync_WithMultipleMedicines_TotalEqualsSumOfRows()
    {
        var service = CreateService(new[]
        {
            new Medicine
            {
                ProductId = 1,
                BrandName = "Paracetamol",
                ReorderLevel = 10,
                Batches = { Batch(1, 1, 100, 20m) }
            },
            new Medicine
            {
                ProductId = 2,
                BrandName = "Amoxicillin",
                ReorderLevel = 10,
                Batches = { Batch(2, 2, 30, 25m) }
            }
        });

        var result = (await service.GetInventoryReportAsync()).ToList();

        result.Should().HaveCount(2);

        var totalValue = result.Sum(r => r.Value);
        var totalQuantity = result.Sum(r => r.Quantity);

        totalValue.Should().Be((100m * 20m) + (30m * 25m));
        totalValue.Should().Be(2750m);
        totalQuantity.Should().Be(130);

        result.First(r => r.ProductId == 1).Value.Should().Be(2000m);
        result.First(r => r.ProductId == 2).Value.Should().Be(750m);
    }
}