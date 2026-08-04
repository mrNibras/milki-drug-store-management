using Xunit;
using FluentAssertions;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Application.DTOs.Sale;
using Microsoft.Extensions.Logging;
using Moq;

namespace MilkiDrugStore.Tests.Application;

public class SaleServiceReferenceNumberTests
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

    public SaleServiceReferenceNumberTests()
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
    public async Task CreateAsync_WithBankTransfer_ReturnsReferenceNumber()
    {
        var medicine = new Medicine { ProductId = 1, BrandName = "Amoxicillin", ReorderLevel = 10 };
        var batch = new MedicineBatch
        {
            BatchId = 1,
            ProductId = 1,
            BatchNumber = "B002",
            QuantityReceived = 100,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            SellingPrice = 50,
            PurchasePrice = 30
        };

        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { medicine }.AsQueryable());
        _batchRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MedicineBatch, bool>>>()))
            .ReturnsAsync(new List<MedicineBatch> { batch }.AsQueryable());

        var request = new CreateSaleRequest
        {
            Items = new List<SaleItemRequest> { new() { ProductId = 1, Quantity = 1 } },
            PaymentMethod = "bank_transfer",
            AmountPaid = 50,
            ReferenceNumber = "TXN-12345"
        };

        var result = await _sut.CreateAsync(request, 1, "Admin");

        result.Should().NotBeNull();
        result.ReferenceNumber.Should().Be("TXN-12345");
        result.PaymentMethod.Should().Be("bank_transfer");
    }

    [Fact]
    public async Task CreateAsync_WithoutReferenceNumber_ReturnsNull()
    {
        var medicine = new Medicine { ProductId = 1, BrandName = "Amoxicillin", ReorderLevel = 10 };
        var batch = new MedicineBatch
        {
            BatchId = 1,
            ProductId = 1,
            BatchNumber = "B002",
            QuantityReceived = 100,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            SellingPrice = 50,
            PurchasePrice = 30
        };

        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { medicine }.AsQueryable());
        _batchRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MedicineBatch, bool>>>()))
            .ReturnsAsync(new List<MedicineBatch> { batch }.AsQueryable());

        var request = new CreateSaleRequest
        {
            Items = new List<SaleItemRequest> { new() { ProductId = 1, Quantity = 1 } },
            PaymentMethod = "cash",
            AmountPaid = 50
        };

        var result = await _sut.CreateAsync(request, 1, "Admin");

        result.Should().NotBeNull();
        result.ReferenceNumber.Should().BeNull();
    }
}
