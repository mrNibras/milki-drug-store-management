using Xunit;
using FluentAssertions;
using Moq;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Application.DTOs.Sale;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Tests.Application;

public class SaleServiceTests
{
    private readonly Mock<IRepository<Sale>> _saleRepo = new();
    private readonly Mock<IRepository<SaleItem>> _saleItemRepo = new();
    private readonly Mock<IRepository<Medicine>> _medicineRepo = new();
    private readonly Mock<IRepository<MedicineBatch>> _batchRepo = new();
    private readonly Mock<IRepository<InventoryTransaction>> _transactionRepo = new();
    private readonly Mock<IRepository<Notification>> _notificationRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly Mock<ILogger<SaleService>> _logger = new();
    private readonly SaleService _sut;

    public SaleServiceTests()
    {
        _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        _sut = new SaleService(
            _saleRepo.Object,
            _saleItemRepo.Object,
            _medicineRepo.Object,
            _batchRepo.Object,
            _transactionRepo.Object,
            _notificationRepo.Object,
            _unitOfWork.Object,
            _auditLog.Object
        );
    }

    [Fact]
    public async Task CreateAsync_WithCashPayment_SetsPaymentStatusPaid()
    {
        var medicine = new Medicine { MedicineId = 1, MedicineName = "Paracetamol", LowStockThreshold = 10 };
        var batch = new MedicineBatch
        {
            BatchId = 1,
            MedicineId = 1,
            BatchNumber = "B001",
            QuantityReceived = 100,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            SellingPrice = 10,
            PurchasePrice = 5
        };

        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { medicine }.AsQueryable());
        _batchRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MedicineBatch, bool>>>()))
            .ReturnsAsync(new List<MedicineBatch> { batch }.AsQueryable());

        var request = new CreateSaleRequest
        {
            Items = new List<SaleItemRequest> { new() { MedicineId = 1, Quantity = 2 } },
            PaymentMethod = "cash",
            AmountPaid = 20
        };

        var result = await _sut.CreateAsync(request, 1);

        result.Should().NotBeNull();
        result.PaymentMethod.Should().Be("cash");
        result.PaymentStatus.Should().Be("paid");
        result.AmountPaid.Should().Be(20);
        result.AmountDue.Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_WithPartialPayment_SetsPaymentStatusPartial()
    {
        var medicine = new Medicine { MedicineId = 1, MedicineName = "Paracetamol", LowStockThreshold = 10 };
        var batch = new MedicineBatch
        {
            BatchId = 1,
            MedicineId = 1,
            BatchNumber = "B001",
            QuantityReceived = 100,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            SellingPrice = 10,
            PurchasePrice = 5
        };

        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { medicine }.AsQueryable());
        _batchRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MedicineBatch, bool>>>()))
            .ReturnsAsync(new List<MedicineBatch> { batch }.AsQueryable());

        var request = new CreateSaleRequest
        {
            Items = new List<SaleItemRequest> { new() { MedicineId = 1, Quantity = 2 } },
            PaymentMethod = "credit",
            AmountPaid = 5
        };

        var result = await _sut.CreateAsync(request, 1);

        result.Should().NotBeNull();
        result.PaymentMethod.Should().Be("credit");
        result.PaymentStatus.Should().Be("partial");
        result.AmountPaid.Should().Be(5);
        result.AmountDue.Should().Be(15);
    }

    [Fact]
    public async Task CreateAsync_WithZeroPayment_SetsPaymentStatusUnpaid()
    {
        var medicine = new Medicine { MedicineId = 1, MedicineName = "Paracetamol", LowStockThreshold = 10 };
        var batch = new MedicineBatch
        {
            BatchId = 1,
            MedicineId = 1,
            BatchNumber = "B001",
            QuantityReceived = 100,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            SellingPrice = 10,
            PurchasePrice = 5
        };

        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { medicine }.AsQueryable());
        _batchRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MedicineBatch, bool>>>()))
            .ReturnsAsync(new List<MedicineBatch> { batch }.AsQueryable());

        var request = new CreateSaleRequest
        {
            Items = new List<SaleItemRequest> { new() { MedicineId = 1, Quantity = 2 } },
            PaymentMethod = "credit",
            AmountPaid = 0
        };

        var result = await _sut.CreateAsync(request, 1);

        result.Should().NotBeNull();
        result.PaymentStatus.Should().Be("unpaid");
        result.AmountDue.Should().Be(20);
    }
}
