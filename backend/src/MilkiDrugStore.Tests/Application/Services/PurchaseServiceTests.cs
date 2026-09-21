using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application.Services;

public class PurchaseServiceTests
{
    private readonly Mock<IRepository<Purchase>> _purchaseRepo = new();
    private readonly Mock<IRepository<PurchaseItem>> _purchaseItemRepo = new();
    private readonly Mock<IRepository<Supplier>> _supplierRepo = new();
    private readonly Mock<IRepository<Medicine>> _medicineRepo = new();
    private readonly Mock<IRepository<MedicineBatch>> _batchRepo = new();
    private readonly Mock<ICatalogService> _catalog = new();
    private readonly Mock<IRepository<InventoryTransaction>> _transactionRepo = new();
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILogger<PurchaseService>> _logger = new();
    private readonly PurchaseService _sut;

    public PurchaseServiceTests()
    {
        _sut = new PurchaseService(
            _purchaseRepo.Object,
            _purchaseItemRepo.Object,
            _medicineRepo.Object,
            _batchRepo.Object,
            _supplierRepo.Object,
            _catalog.Object,
            _transactionRepo.Object,
            _unitOfWork.Object,
            _auditLog.Object,
            Mock.Of<ILogger<PurchaseService>>());

        _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);
        _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        var branchRepo = new Mock<IRepository<Branch>>();
        branchRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Branch>
        {
            new Branch { BranchId = 1, BranchName = "Test Branch" }
        }.AsQueryable());
        _unitOfWork.Setup(u => u.Branches).Returns(branchRepo.Object);

        _supplierRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier>().AsQueryable());

        _purchaseRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Purchase>().AsQueryable());

        _purchaseItemRepo.Setup(r => r.AddAsync(It.IsAny<PurchaseItem>())).ReturnsAsync(new PurchaseItem { PurchaseItemId = 1 });
        _transactionRepo.Setup(r => r.AddAsync(It.IsAny<InventoryTransaction>())).ReturnsAsync(new InventoryTransaction { TransactionId = 1 });
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Purchase_With_Paid_Status_When_AmountPaid_Equals_Total()
    {
        var supplier = new Supplier { SupplierId = 1, SupplierName = "Test Supplier" };
        _supplierRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier> { supplier }.AsQueryable());

        var request = new MilkiDrugStore.Application.DTOs.Purchase.CreatePurchaseRequest
        {
            SupplierId = 1,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 1000,
            Items = new List<MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest>
            {
                new MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest { ProductId = 1, BatchNumber = "B1", Quantity = 10, PurchasePrice = 100, SellingPrice = 150, ExpiryDate = DateTime.UtcNow.AddYears(1) }
            }
        };

        Purchase? capturedPurchase = null;
        _purchaseRepo.Setup(r => r.AddAsync(It.IsAny<Purchase>()))
            .ReturnsAsync((Purchase p) => { capturedPurchase = p; p.PurchaseId = 1; return p; });
        _purchaseRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Purchase, bool>>>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Purchase, bool>> predicate) =>
            {
                if (capturedPurchase == null) return new List<Purchase>().AsQueryable();
                return new List<Purchase> { capturedPurchase }.AsQueryable().Where(predicate.Compile()).AsQueryable();
            });

        _medicineRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Medicine { ProductId = 1 });
        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { new Medicine { ProductId = 1 } }.AsQueryable());

        _batchRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MedicineBatch?)null);
        _batchRepo.Setup(r => r.AddAsync(It.IsAny<MedicineBatch>())).ReturnsAsync(new MedicineBatch { BatchId = 1 });

        var result = await _sut.CreateAsync(request, 1);

        Assert.NotNull(result);
        Assert.Equal(1000, result.TotalAmount);
        Assert.Equal(1000, result.AmountPaid);
        Assert.Equal(0, result.AmountDue);
        Assert.Equal("paid", result.PaymentStatus);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Purchase_With_Partial_Status_When_Partially_Paid()
    {
        var supplier = new Supplier { SupplierId = 1, SupplierName = "Test Supplier" };
        _supplierRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier> { supplier }.AsQueryable());

        var request = new MilkiDrugStore.Application.DTOs.Purchase.CreatePurchaseRequest
        {
            SupplierId = 1,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "cash",
            AmountPaid = 500,
            Items = new List<MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest>
            {
                new MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest { ProductId = 1, BatchNumber = "B1", Quantity = 10, PurchasePrice = 100, SellingPrice = 150, ExpiryDate = DateTime.UtcNow.AddYears(1) }
            }
        };

        Purchase? capturedPurchase = null;
        _purchaseRepo.Setup(r => r.AddAsync(It.IsAny<Purchase>()))
            .ReturnsAsync((Purchase p) => { capturedPurchase = p; p.PurchaseId = 1; return p; });
        _purchaseRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Purchase, bool>>>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Purchase, bool>> predicate) =>
            {
                if (capturedPurchase == null) return new List<Purchase>().AsQueryable();
                return new List<Purchase> { capturedPurchase }.AsQueryable().Where(predicate.Compile()).AsQueryable();
            });

        _medicineRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Medicine { ProductId = 1 });
        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { new Medicine { ProductId = 1 } }.AsQueryable());

        _batchRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MedicineBatch?)null);
        _batchRepo.Setup(r => r.AddAsync(It.IsAny<MedicineBatch>())).ReturnsAsync(new MedicineBatch { BatchId = 1 });

        var result = await _sut.CreateAsync(request, 1);

        Assert.NotNull(result);
        Assert.Equal(500, result.AmountPaid);
        Assert.Equal(500, result.AmountDue);
        Assert.Equal("partial", result.PaymentStatus);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Purchase_With_Unpaid_Status_When_Not_Paid()
    {
        var supplier = new Supplier { SupplierId = 1, SupplierName = "Test Supplier" };
        _supplierRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier> { supplier }.AsQueryable());

        var request = new MilkiDrugStore.Application.DTOs.Purchase.CreatePurchaseRequest
        {
            SupplierId = 1,
            PurchaseDate = DateTime.UtcNow,
            PaymentMethod = "credit",
            AmountPaid = 0,
            Items = new List<MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest>
            {
                new MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest { ProductId = 1, BatchNumber = "B1", Quantity = 10, PurchasePrice = 100, SellingPrice = 150, ExpiryDate = DateTime.UtcNow.AddYears(1) }
            }
        };

        Purchase? capturedPurchase = null;
        _purchaseRepo.Setup(r => r.AddAsync(It.IsAny<Purchase>()))
            .ReturnsAsync((Purchase p) => { capturedPurchase = p; p.PurchaseId = 1; return p; });
        _purchaseRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Purchase, bool>>>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Purchase, bool>> predicate) =>
            {
                if (capturedPurchase == null) return new List<Purchase>().AsQueryable();
                return new List<Purchase> { capturedPurchase }.AsQueryable().Where(predicate.Compile()).AsQueryable();
            });

        _medicineRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Medicine { ProductId = 1 });
        _medicineRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Medicine, bool>>>()))
            .ReturnsAsync(new List<Medicine> { new Medicine { ProductId = 1 } }.AsQueryable());

        _batchRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((MedicineBatch?)null);
        _batchRepo.Setup(r => r.AddAsync(It.IsAny<MedicineBatch>())).ReturnsAsync(new MedicineBatch { BatchId = 1 });

        var result = await _sut.CreateAsync(request, 1);

        Assert.NotNull(result);
        Assert.Equal(0, result.AmountPaid);
        Assert.Equal(1000, result.AmountDue);
        Assert.Equal("unpaid", result.PaymentStatus);
    }
}
