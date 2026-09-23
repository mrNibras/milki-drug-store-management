using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Exceptions;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application.Services;

public class FEFOTests
{
    private readonly Mock<IRepository<Sale>> _saleRepo = new();
    private readonly Mock<IRepository<SaleItem>> _saleItemRepo = new();
    private readonly Mock<IRepository<Medicine>> _medicineRepo = new();
    private readonly Mock<IRepository<MedicineBatch>> _batchRepo = new();
    private readonly Mock<IRepository<InventoryTransaction>> _transactionRepo = new();
    private readonly Mock<IRepository<Notification>> _notificationRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly Mock<IRepository<Settings>> _settingsRepo = new();
    private readonly Mock<ILogger<SaleService>> _logger = new();
    private readonly SaleService _sut;

    public FEFOTests()
    {
        _sut = new SaleService(
            _saleRepo.Object,
            _saleItemRepo.Object,
            _medicineRepo.Object,
            _batchRepo.Object,
            new Mock<ICosmeticRepository>().Object,
            new Mock<IRepository<CosmeticBatch>>().Object,
            _transactionRepo.Object,
            _notificationRepo.Object,
            _settingsRepo.Object,
            _unitOfWork.Object,
            _auditLog.Object
        );
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Batch_Not_Found()
    {
        _medicineRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Medicine { ProductId = 1 });
        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { new Medicine { ProductId = 1 } }.AsQueryable());
        _batchRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineBatch, bool>>>()))
            .Returns(Task.FromResult((System.Linq.IQueryable<MedicineBatch>)new List<MedicineBatch>().AsQueryable()));

        var request = new MilkiDrugStore.Application.DTOs.Sale.CreateSaleRequest
        {
            Items = new List<MilkiDrugStore.Application.DTOs.Sale.SaleItemRequest>
            {
                new MilkiDrugStore.Application.DTOs.Sale.SaleItemRequest { ProductId = 1, Quantity = 5 }
            },
            PaymentMethod = "cash",
            AmountPaid = 500
        };

        await Assert.ThrowsAsync<InsufficientStockException>(() => _sut.CreateAsync(request, 1, "Pharmacist", null));
    }

    [Fact]
    public async Task GetAvailableBatchesAsync_Should_Filter_By_Branch()
    {
        var method = typeof(SaleService).GetMethod("GetAvailableBatchesAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var batches = new List<MedicineBatch>
        {
            new MedicineBatch { ProductId = 1, BatchId = 1, BatchNumber = "B1", QuantityReceived = 10, QuantityIssued = 0, QuantityDamaged = 0, QuantityExpired = 0, ExpiryDate = DateTime.UtcNow.AddMonths(12), BranchId = 1 },
            new MedicineBatch { ProductId = 1, BatchId = 2, BatchNumber = "B2", QuantityReceived = 10, QuantityIssued = 0, QuantityDamaged = 0, QuantityExpired = 0, ExpiryDate = DateTime.UtcNow.AddMonths(3), BranchId = 2 }
        };
        _batchRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<MedicineBatch, bool>>>()))
            .Returns(Task.FromResult((System.Linq.IQueryable<MedicineBatch>)batches.AsQueryable()));

        var task = (Task<List<MedicineBatch>>)method!.Invoke(_sut, new object[] { 1, 1, null })!;
        var result = await task;

        Assert.Single(result);
        Assert.Equal(1, result.First().BatchId);
    }
}
