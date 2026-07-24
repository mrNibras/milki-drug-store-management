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
    private readonly Mock<IRepository<Category>> _categoryRepo = new();
    private readonly Mock<IRepository<UnitType>> _unitTypeRepo = new();
    private readonly Mock<IRepository<InventoryTransaction>> _transactionRepo = new();
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly PurchaseService _sut;

    public PurchaseServiceTests()
    {
        _sut = new PurchaseService(
            _purchaseRepo.Object,
            _purchaseItemRepo.Object,
            _medicineRepo.Object,
            _batchRepo.Object,
            _supplierRepo.Object,
            _categoryRepo.Object,
            _unitTypeRepo.Object,
            _transactionRepo.Object,
            _unitOfWork.Object,
            _auditLog.Object
        );
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Purchase_With_Paid_Status_When_AmountPaid_Equals_Total()
    {
        var supplier = new Supplier { SupplierId = 1, SupplierName = "Test Supplier" };
        _supplierRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier> { supplier }.AsQueryable());

        var request = new MilkiDrugStore.Application.DTOs.Purchase.CreatePurchaseRequest
        {
            SupplierId = 1,
            PurchaseDate = DateTime.Now,
            PaymentMethod = "cash",
            AmountPaid = 1000,
            Items = new List<MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest>
            {
                new MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest { MedicineId = 1, BatchNumber = "B1", Quantity = 10, PurchasePrice = 100, SellingPrice = 150, ExpiryDate = DateTime.Now.AddYears(1) }
            }
        };

        var result = await _sut.CreateAsync(request, 1);

        Assert.NotNull(result);
        Assert.Equal(1, result.SupplierId);
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
            PurchaseDate = DateTime.Now,
            PaymentMethod = "cash",
            AmountPaid = 500,
            Items = new List<MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest>
            {
                new MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest { MedicineId = 1, BatchNumber = "B1", Quantity = 10, PurchasePrice = 100, SellingPrice = 150, ExpiryDate = DateTime.Now.AddYears(1) }
            }
        };

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
            PurchaseDate = DateTime.Now,
            PaymentMethod = "credit",
            AmountPaid = 0,
            Items = new List<MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest>
            {
                new MilkiDrugStore.Application.DTOs.Purchase.PurchaseItemRequest { MedicineId = 1, BatchNumber = "B1", Quantity = 10, PurchasePrice = 100, SellingPrice = 150, ExpiryDate = DateTime.Now.AddYears(1) }
            }
        };

        var result = await _sut.CreateAsync(request, 1);

        Assert.NotNull(result);
        Assert.Equal(0, result.AmountPaid);
        Assert.Equal(1000, result.AmountDue);
        Assert.Equal("unpaid", result.PaymentStatus);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Purchases()
    {
        var purchases = new List<Purchase>
        {
            new Purchase { PurchaseId = 1, PurchaseNumber = "PUR-2026-00001", SupplierId = 1, TotalAmount = 1000, AmountPaid = 1000, AmountDue = 0, PaymentStatus = "paid" },
            new Purchase { PurchaseId = 2, PurchaseNumber = "PUR-2026-00002", SupplierId = 1, TotalAmount = 2000, AmountPaid = 1000, AmountDue = 1000, PaymentStatus = "partial" }
        };
        _purchaseRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(purchases.AsQueryable());

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }
}
