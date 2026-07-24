using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application.Services;

public class MedicineServiceTests
{
    private readonly Mock<IRepository<Medicine>> _medicineRepo = new();
    private readonly Mock<IRepository<Category>> _categoryRepo = new();
    private readonly Mock<IRepository<MedicineBatch>> _batchRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditLogService> _auditLog = new();
    private readonly MedicineService _sut;

    public MedicineServiceTests()
    {
        _sut = new MedicineService(
            _medicineRepo.Object,
            _categoryRepo.Object,
            _batchRepo.Object,
            _unitOfWork.Object,
            _auditLog.Object
        );
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Medicines()
    {
        var medicines = new List<Medicine>
        {
            new Medicine { MedicineId = 1, MedicineName = "Aspirin", GenericName = "Acetylsalicylic Acid", CategoryId = 1 },
            new Medicine { MedicineId = 2, MedicineName = "Paracetamol", GenericName = "Acetaminophen", CategoryId = 1 }
        };
        _medicineRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(medicines.AsQueryable());

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_Should_Filter_By_Search_Term()
    {
        var medicines = new List<Medicine>
        {
            new Medicine { MedicineId = 1, MedicineName = "Aspirin", GenericName = "Acetylsalicylic Acid", CategoryId = 1 },
            new Medicine { MedicineId = 2, MedicineName = "Paracetamol", GenericName = "Acetaminophen", CategoryId = 1 }
        };
        _medicineRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(medicines.AsQueryable());

        var result = await _sut.GetAllAsync(search: "Aspirin");

        Assert.Single(result);
        Assert.Equal("Aspirin", result.First().MedicineName);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Medicine_When_Exists()
    {
        var medicine = new Medicine { MedicineId = 1, MedicineName = "Aspirin", GenericName = "Acetylsalicylic Acid", CategoryId = 1 };
        _medicineRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Medicine> { medicine }.AsQueryable());

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Aspirin", result.MedicineName);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_Medicine()
    {
        var request = new MilkiDrugStore.Application.DTOs.Medicine.CreateMedicineRequest
        {
            MedicineName = "NewMedicine",
            GenericName = "Generic",
            CategoryId = 1,
            UnitTypeId = 1,
            LowStockThreshold = 10
        };

        var createdMedicine = new Medicine { MedicineId = 1, MedicineName = "NewMedicine" };
        _medicineRepo.Setup(r => r.AddAsync(It.IsAny<Medicine>())).ReturnsAsync(createdMedicine);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateAsync(request, 1);

        Assert.NotNull(result);
        Assert.Equal("NewMedicine", result.MedicineName);
    }
}
