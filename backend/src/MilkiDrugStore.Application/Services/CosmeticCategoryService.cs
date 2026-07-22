using MilkiDrugStore.Application.DTOs.Cosmetic;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Application.Services;

public class CosmeticCategoryService : ICosmeticCategoryService
{
    private readonly IRepository<CosmeticCategory> _categoryRepo;
    private readonly IAuditLogService _auditLog;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CosmeticCategoryService> _logger;

    public CosmeticCategoryService(
        IRepository<CosmeticCategory> categoryRepo,
        IAuditLogService auditLog,
        IUnitOfWork unitOfWork,
        ILogger<CosmeticCategoryService> logger)
    {
        _categoryRepo = categoryRepo;
        _auditLog = auditLog;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<CosmeticCategoryResponse>> GetAllAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return categories.OrderBy(c => c.Name).Select(c => new CosmeticCategoryResponse
        {
            CosmeticCategoryId = c.CosmeticCategoryId,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<CosmeticCategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category == null) return null;

        return new CosmeticCategoryResponse
        {
            CosmeticCategoryId = category.CosmeticCategoryId,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }

    public async Task<CosmeticCategoryResponse> CreateAsync(CreateCosmeticCategoryRequest request, int userId)
    {
        var category = new CosmeticCategory
        {
            Name = request.Name,
            Description = request.Description
        };

        await _categoryRepo.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created cosmetic category: {category.Name}", "CosmeticCategories", category.CosmeticCategoryId);

        return new CosmeticCategoryResponse
        {
            CosmeticCategoryId = category.CosmeticCategoryId,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }

    public async Task<CosmeticCategoryResponse?> UpdateAsync(int id, UpdateCosmeticCategoryRequest request, int userId)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category == null) return null;

        category.Name = request.Name;
        category.Description = request.Description;
        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        await _categoryRepo.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Updated cosmetic category: {category.Name}", "CosmeticCategories", category.CosmeticCategoryId);

        return new CosmeticCategoryResponse
        {
            CosmeticCategoryId = category.CosmeticCategoryId,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        if (category == null) return;

        category.IsActive = false;
        await _categoryRepo.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Deactivated cosmetic category: {category.Name}", "CosmeticCategories", category.CosmeticCategoryId);
    }
}
