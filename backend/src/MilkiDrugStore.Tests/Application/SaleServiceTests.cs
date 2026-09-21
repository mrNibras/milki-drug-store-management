using System.Linq.Expressions;
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
    private readonly Mock<IRepository<Settings>> _settingsRepo = new();
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
            _settingsRepo.Object,
            _unitOfWork.Object,
            _auditLog.Object
        );
    }

    [Fact]
    public async Task CreateAsync_WithCashPayment_SetsPaymentStatusPaid()
    {
        var medicine = new Medicine { ProductId = 1, BrandName = "Paracetamol", ReorderLevel = 10 };
        var batch = new MedicineBatch
        {
            BatchId = 1,
            ProductId = 1,
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
            Items = new List<SaleItemRequest> { new() { ProductId = 1, Quantity = 2 } },
            PaymentMethod = "cash",
            AmountPaid = 20
        };

        var result = await _sut.CreateAsync(request, 1, "Admin");

        result.Should().NotBeNull();
        result.PaymentMethod.Should().Be("cash");
        result.PaymentStatus.Should().Be("paid");
        result.AmountPaid.Should().Be(20);
        result.AmountDue.Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_WithPartialPayment_SetsPaymentStatusPartial()
    {
        var medicine = new Medicine { ProductId = 1, BrandName = "Paracetamol", ReorderLevel = 10 };
        var batch = new MedicineBatch
        {
            BatchId = 1,
            ProductId = 1,
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
            Items = new List<SaleItemRequest> { new() { ProductId = 1, Quantity = 2 } },
            PaymentMethod = "credit",
            AmountPaid = 5
        };

        var result = await _sut.CreateAsync(request, 1, "Admin");

        result.Should().NotBeNull();
        result.PaymentMethod.Should().Be("credit");
        result.PaymentStatus.Should().Be("partial");
        result.AmountPaid.Should().Be(5);
        result.AmountDue.Should().Be(15);
    }

    [Fact]
    public async Task CreateAsync_WithZeroPayment_SetsPaymentStatusUnpaid()
    {
        var medicine = new Medicine { ProductId = 1, BrandName = "Paracetamol", ReorderLevel = 10 };
        var batch = new MedicineBatch
        {
            BatchId = 1,
            ProductId = 1,
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
            Items = new List<SaleItemRequest> { new() { ProductId = 1, Quantity = 2 } },
            PaymentMethod = "credit",
            AmountPaid = 0
        };

        var result = await _sut.CreateAsync(request, 1, "Admin");

        result.Should().NotBeNull();
        result.PaymentStatus.Should().Be("unpaid");
        result.AmountDue.Should().Be(20);
    }

    [Fact]
    public async Task CreateAsync_WithDiscount_PreservesDiscountForAdmin()
    {
        var medicine = new Medicine { ProductId = 1, BrandName = "Paracetamol", ReorderLevel = 10 };
        var batch = new MedicineBatch
        {
            BatchId = 1,
            ProductId = 1,
            BatchNumber = "B001",
            QuantityReceived = 100,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            SellingPrice = 100,
            PurchasePrice = 50
        };

        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { medicine }.AsQueryable());
        _batchRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MedicineBatch, bool>>>()))
            .ReturnsAsync(new List<MedicineBatch> { batch }.AsQueryable());

        var request = new CreateSaleRequest
        {
            Items = new List<SaleItemRequest> { new() { ProductId = 1, Quantity = 2, DiscountAmount = 10 } },
            PaymentMethod = "cash",
            AmountPaid = 180,
            DiscountReason = "Customer loyalty"
        };

        var result = await _sut.CreateAsync(request, 1, "Admin");

        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(180);
        result.TotalDiscount.Should().Be(20);
        result.Items.Should().HaveCount(1);
        result.Items[0].DiscountAmount.Should().Be(10);
        result.Items[0].UnitPrice.Should().Be(100);
    }

    [Fact]
    public async Task CreateAsync_WithDiscount_CapsDiscountForPharmacist()
    {
        var medicine = new Medicine { ProductId = 1, BrandName = "Paracetamol", ReorderLevel = 10 };
        var batch = new MedicineBatch
        {
            BatchId = 1,
            ProductId = 1,
            BatchNumber = "B001",
            QuantityReceived = 100,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            SellingPrice = 100,
            PurchasePrice = 50
        };

        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { medicine }.AsQueryable());
        _batchRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MedicineBatch, bool>>>()))
            .ReturnsAsync(new List<MedicineBatch> { batch }.AsQueryable());

        var request = new CreateSaleRequest
        {
            Items = new List<SaleItemRequest> { new() { ProductId = 1, Quantity = 2, DiscountAmount = 10 } },
            PaymentMethod = "cash",
            AmountPaid = 190,
            DiscountReason = "Customer loyalty"
        };

        var result = await _sut.CreateAsync(request, 1, "Pharmacist");

        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(190);
        result.TotalDiscount.Should().Be(10);
        result.Items.Should().HaveCount(1);
        result.Items[0].DiscountAmount.Should().Be(5);
        result.Items[0].UnitPrice.Should().Be(100);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSalesWithItemsAndTotals()
    {
        var user = new User { UserId = 1, FullName = "Admin" };
        var medicine = new Medicine { ProductId = 1, BrandName = "Paracetamol" };
        var batch = new MedicineBatch { BatchId = 1, ProductId = 1, BatchNumber = "B001", SellingPrice = 100, PurchasePrice = 50 };
        var sale = new Sale
        {
            SaleId = 1,
            SaleNumber = "SAL-2026-00001",
            SaleDate = DateTime.UtcNow,
            TotalAmount = 200,
            TotalProfit = 100,
            TotalDiscount = 0,
            UserId = 1,
            PaymentMethod = "cash",
            PaymentStatus = "paid",
            AmountPaid = 200,
            AmountDue = 0,
            User = user,
            Items = new List<SaleItem>
            {
                new SaleItem
                {
                    SaleItemId = 1,
                    SaleId = 1,
                    ProductId = 1,
                    BatchId = 1,
                    Quantity = 2,
                    UnitPrice = 100,
                    DiscountAmount = 0,
                    PurchasePrice = 50,
                    Profit = 100,
                    SubTotal = 200,
                    Medicine = medicine,
                    Batch = batch
                }
            }
        };

        _saleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new TestAsyncEnumerable<Sale>(new List<Sale> { sale }));

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(1);
        var s = result.First();
        s.TotalAmount.Should().Be(200);
        s.TotalProfit.Should().Be(100);
        s.UserName.Should().Be("Admin");
        s.Items.Should().HaveCount(1);
        s.Items[0].BrandName.Should().Be("Paracetamol");
        s.Items[0].BatchNumber.Should().Be("B001");
        s.Items[0].DiscountAmount.Should().Be(0);
    }
}
