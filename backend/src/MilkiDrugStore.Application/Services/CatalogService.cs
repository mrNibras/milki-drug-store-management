using MilkiDrugStore.Application.DTOs.Catalog;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Catalog;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Exceptions;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;

namespace MilkiDrugStore.Application.Services;

public class CatalogService : ICatalogService
{
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<UnitType> _unitTypeRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;

    public CatalogService(
        IRepository<Category> categoryRepo,
        IRepository<UnitType> unitTypeRepo,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog)
    {
        _categoryRepo = categoryRepo;
        _unitTypeRepo = unitTypeRepo;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    public async Task<IEnumerable<CatalogOptionDto>> GetCategoryOptionsAsync()
    {
        var customs = (await _categoryRepo.GetAllAsync()).ToList();
        var options = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in customs)
        {
            if (!c.IsActive || string.IsNullOrWhiteSpace(c.Name))
                continue;

            var name = c.Name.Trim();
            if (MedicineCatalog.FindCategoryId(name) is not null)
                continue;

            options.TryAdd(name, c.CategoryId);
        }

        return MedicineCatalog.Categories
            .Select(c => new CatalogOptionDto { Id = c.Id, Name = c.Name, IsBuiltIn = true })
            .Concat(options
                .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => new CatalogOptionDto { Id = kv.Value, Name = kv.Key, IsBuiltIn = false }))
            .ToList();
    }

    public async Task<IEnumerable<CatalogOptionDto>> GetUnitTypeOptionsAsync()
    {
        var unitTypes = (await _unitTypeRepo.GetAllAsync()).ToList();
        var options = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var u in unitTypes)
        {
            if (!u.IsActive || string.IsNullOrWhiteSpace(u.Name))
                continue;

            var name = u.Name.Trim();
            if (MedicineCatalog.FindUnitTypeId(name) is not null)
                continue;

            options.TryAdd(name, u.UnitTypeId);
        }

        return MedicineCatalog.UnitTypes
            .Select(u => new CatalogOptionDto { Id = u.Id, Name = u.Name, IsBuiltIn = true })
            .Concat(options
                .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => new CatalogOptionDto { Id = kv.Value, Name = kv.Key, IsBuiltIn = false }))
            .ToList();
    }

    public async Task<CatalogOptionDto> CreateCustomCategoryAsync(string name, int userId)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException("Category name is required.");

        if (MedicineCatalog.FindCategoryId(trimmed) is not null)
            throw new DomainException($"\"{trimmed}\" is a built-in category.");

        var existing = await _categoryRepo.FindAsync(c => c.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing.Any())
            throw new DomainException($"Category \"{trimmed}\" already exists.");

        var category = new Category { Name = trimmed, IsActive = true, CreatedAt = DateTime.Now };
        await _categoryRepo.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created custom category: {category.Name}", "Categories", category.CategoryId);

        return new CatalogOptionDto { Id = category.CategoryId, Name = category.Name, IsBuiltIn = false };
    }

    public async Task<CatalogOptionDto> CreateCustomUnitTypeAsync(string name, int userId)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException("Unit type name is required.");

        if (MedicineCatalog.FindUnitTypeId(trimmed) is not null)
            throw new DomainException($"\"{trimmed}\" is a built-in unit type.");

        var existing = await _unitTypeRepo.FindAsync(u => u.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing.Any())
            throw new DomainException($"Unit type \"{trimmed}\" already exists.");

        var unitType = new UnitType { Name = trimmed, IsActive = true };
        await _unitTypeRepo.AddAsync(unitType);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created custom unit type: {unitType.Name}", "UnitTypes", unitType.UnitTypeId);

        return new CatalogOptionDto { Id = unitType.UnitTypeId, Name = unitType.Name, IsBuiltIn = false };
    }

    public async Task<int?> ResolveCategoryIdAsync(string? name, int userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var trimmed = name.Trim();

        var builtIn = MedicineCatalog.FindCategoryId(trimmed);
        if (builtIn.HasValue)
            return builtIn.Value;

        var existing = await _categoryRepo.FindAsync(c => c.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing.Any())
            return existing.First().CategoryId;

        var category = new Category { Name = trimmed, IsActive = true, CreatedAt = DateTime.Now };
        await _categoryRepo.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created custom category: {category.Name}", "Categories", category.CategoryId);

        return category.CategoryId;
    }

    public async Task<int?> ResolveUnitTypeIdAsync(string? name, int userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var trimmed = name.Trim();

        var builtIn = MedicineCatalog.FindUnitTypeId(trimmed);
        if (builtIn.HasValue)
            return builtIn.Value;

        var existing = await _unitTypeRepo.FindAsync(u => u.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing.Any())
            return existing.First().UnitTypeId;

        var unitType = new UnitType { Name = trimmed, IsActive = true };
        await _unitTypeRepo.AddAsync(unitType);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created custom unit type: {unitType.Name}", "UnitTypes", unitType.UnitTypeId);

        return unitType.UnitTypeId;
    }

    public async Task<bool> IsValidCategoryIdAsync(int id)
    {
        if (MedicineCatalog.IsBuiltInCategoryId(id))
            return true;

        return await _categoryRepo.GetByIdAsync(id) != null;
    }

    public async Task<bool> IsValidUnitTypeIdAsync(int id)
    {
        if (MedicineCatalog.IsBuiltInUnitTypeId(id))
            return true;

        return await _unitTypeRepo.GetByIdAsync(id) != null;
    }

    public async Task<string> GetCategoryNameAsync(int id)
    {
        var builtIn = MedicineCatalog.GetCategoryName(id);
        if (builtIn is not null)
            return builtIn;

        var category = await _categoryRepo.GetByIdAsync(id);
        return category?.Name ?? "";
    }

    public async Task<string> GetUnitTypeNameAsync(int id)
    {
        var builtIn = MedicineCatalog.GetUnitTypeName(id);
        if (builtIn is not null)
            return builtIn;

        var unitType = await _unitTypeRepo.GetByIdAsync(id);
        return unitType?.Name ?? "";
    }

    public async Task<IDictionary<int, string>> GetCategoryNamesAsync(IEnumerable<int> ids)
    {
        var result = new Dictionary<int, string>();
        var missing = new List<int>();

        foreach (var id in ids.Distinct())
        {
            var builtIn = MedicineCatalog.GetCategoryName(id);
            if (builtIn is not null)
                result[id] = builtIn;
            else
                missing.Add(id);
        }

        if (missing.Count > 0)
        {
            var customs = (await _categoryRepo.FindAsync(c => missing.Contains(c.CategoryId))).ToList();
            foreach (var c in customs)
                result[c.CategoryId] = c.Name;
        }

        return result;
    }

    public async Task<IDictionary<int, string>> GetUnitTypeNamesAsync(IEnumerable<int> ids)
    {
        var result = new Dictionary<int, string>();
        var missing = new List<int>();

        foreach (var id in ids.Distinct())
        {
            var builtIn = MedicineCatalog.GetUnitTypeName(id);
            if (builtIn is not null)
                result[id] = builtIn;
            else
                missing.Add(id);
        }

        if (missing.Count > 0)
        {
            var customs = (await _unitTypeRepo.FindAsync(u => missing.Contains(u.UnitTypeId))).ToList();
            foreach (var u in customs)
                result[u.UnitTypeId] = u.Name;
        }

        return result;
    }
}
