using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Catalog;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application.Services;

/// <summary>
/// Cosmetic built-in categories use their own negative id range (-100..). These tests
/// pin the catalog resolver behaviour that caused cosmetic category names to come back
/// as an empty string, and guard the medicine behaviour against regression.
/// </summary>
public class CosmeticCategoryResolutionTests
{
    private readonly Mock<IRepository<Category>> _categoryRepo = new();
    private readonly Mock<IRepository<UnitType>> _unitTypeRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CatalogService _sut;

    public CosmeticCategoryResolutionTests()
    {
        _sut = new CatalogService(
            _categoryRepo.Object,
            _unitTypeRepo.Object,
            _unitOfWork.Object,
            Mock.Of<IAuditLogService>());

        // No custom (persisted) category matches any probed id unless a test says so.
        _categoryRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(new List<Category>().AsQueryable());
        _categoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);
    }

    // --- The actual bug ------------------------------------------------

    [Theory]
    [InlineData(-100, "Hair Care")]
    [InlineData(-101, "Skin Care")]
    [InlineData(-102, "Bath & Body")]
    [InlineData(-103, "Oral Care")]
    [InlineData(-104, "Baby Care")]
    [InlineData(-105, "Makeup")]
    [InlineData(-106, "Fragrance")]
    [InlineData(-107, "Feminine Care")]
    public void GetCategoryNamesAsync_ResolvesEveryBuiltInCosmeticCategory(int id, string expected)
    {
        var names = _sut.GetCategoryNamesAsync(new[] { id }).Result;

        Assert.Equal(expected, names[id]);
    }

    [Fact]
    public void GetCategoryNamesAsync_ResolvesOtherBuiltInCosmeticCategory()
    {
        var names = _sut.GetCategoryNamesAsync(new[] { CosmeticCatalog.Categories.First(c => c.Name == CosmeticCatalog.OtherLabel).Id }).Result;

        Assert.Equal(CosmeticCatalog.OtherLabel, names[CosmeticCatalog.Categories.First(c => c.Name == CosmeticCatalog.OtherLabel).Id]);
    }

    [Fact]
    public void GetCategoryNameAsync_ResolvesBuiltInCosmeticCategory()
    {
        Assert.Equal("Skin Care", _sut.GetCategoryNameAsync(-101).Result);
    }

    [Fact]
    public void IsValidCategoryIdAsync_AcceptsBuiltInCosmeticCategory()
    {
        // Previously rejected, which made cosmetic create/update throw
        // "A valid category is required." for a perfectly valid category.
        Assert.True(_sut.IsValidCategoryIdAsync(-101).Result);
    }

    [Fact]
    public void GetCategoryNamesAsync_MixesMedicineAndCosmeticCategories()
    {
        // Both product types are grouped in one inventory-by-category view.
        var names = _sut.GetCategoryNamesAsync(new[] { -1, -101, -105 }).Result;

        Assert.Equal("Antibiotics", names[-1]);
        Assert.Equal("Skin Care", names[-101]);
        Assert.Equal("Makeup", names[-105]);
    }

    // --- Medicine regression -------------------------------------------

    [Theory]
    [InlineData(-1, "Antibiotics")]
    [InlineData(-10, "Vitamins and Minerals")]
    [InlineData(-15, "Other")]
    public void GetCategoryNamesAsync_MedicineCategoriesUnchanged(int id, string expected)
    {
        Assert.Equal(expected, _sut.GetCategoryNamesAsync(new[] { id }).Result[id]);
    }

    [Fact]
    public void IsValidCategoryIdAsync_StillAcceptsMedicineCategory()
    {
        Assert.True(_sut.IsValidCategoryIdAsync(-1).Result);
    }

    [Fact]
    public void GetCategoryNameAsync_MedicineCategoryUnchanged()
    {
        Assert.Equal("Antibiotics", _sut.GetCategoryNameAsync(-1).Result);
    }

    // --- Custom (persisted) categories still work ----------------------

    [Fact]
    public void GetCategoryNamesAsync_ResolvesCustomCategoryFromDatabase()
    {
        var custom = new Category { CategoryId = 77, Name = "Custom Line", IsActive = true };
        _categoryRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(new List<Category> { custom }.AsQueryable());

        Assert.Equal("Custom Line", _sut.GetCategoryNamesAsync(new[] { 77 }).Result[77]);
    }

    [Fact]
    public void IsValidCategoryIdAsync_RejectsUnknownId()
    {
        Assert.False(_sut.IsValidCategoryIdAsync(4242).Result);
    }
}