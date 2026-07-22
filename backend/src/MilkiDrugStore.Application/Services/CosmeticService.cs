using MilkiDrugStore.Application.DTOs.Cosmetic;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Domain.Exceptions;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Application.Services;

public class CosmeticService : ICosmeticService
{
    private readonly ICosmeticRepository _cosmeticRepo;
    private readonly IRepository<CosmeticBatch> _batchRepo;
    private readonly IRepository<CosmeticCategory> _categoryRepo;
    private readonly IRepository<UnitType> _unitTypeRepo;
    private readonly IAuditLogService _auditLog;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CosmeticService> _logger;

    public CosmeticService(
        ICosmeticRepository cosmeticRepo,
        IRepository<CosmeticBatch> batchRepo,
        IRepository<CosmeticCategory> categoryRepo,
        IRepository<UnitType> unitTypeRepo,
        IAuditLogService auditLog,
        IUnitOfWork unitOfWork,
        ILogger<CosmeticService> logger)
    {
        _cosmeticRepo = cosmeticRepo;
        _batchRepo = batchRepo;
        _categoryRepo = categoryRepo;
        _unitTypeRepo = unitTypeRepo;
        _auditLog = auditLog;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<CosmeticResponse>> GetAllAsync(string? search = null, int? categoryId = null)
    {
        var query = (await _cosmeticRepo.GetAllAsync()).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.ProductName.Contains(search) || c.Description.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(c => c.CosmeticCategoryId == categoryId.Value);

        var cosmetics = query
            .Include(c => c.CosmeticCategory)
            .Include(c => c.UnitType)
            .Include(c => c.Batches)
            .OrderBy(c => c.ProductName)
            .ToList();

        return cosmetics.Select(MapToResponse);
    }

    public async Task<CosmeticResponse?> GetByIdAsync(int id)
    {
        var cosmetics = await _cosmeticRepo.FindAsync(c => c.CosmeticId == id);
        var cosmetic = cosmetics.Include(c => c.CosmeticCategory).Include(c => c.UnitType).Include(c => c.Batches).FirstOrDefault();
        if (cosmetic == null) return null;
        return MapToResponse(cosmetic);
    }

    public async Task<CosmeticResponse> CreateAsync(CreateCosmeticRequest request, int userId)
    {
        var cosmetic = new Cosmetic
        {
            ProductName = request.ProductName,
            Description = request.Description,
            CosmeticCategoryId = request.CosmeticCategoryId,
            UnitTypeId = request.UnitTypeId,
            Price = request.Price,
            IsActive = true
        };

        await _cosmeticRepo.AddAsync(cosmetic);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created cosmetic: {cosmetic.ProductName}", "Cosmetics", cosmetic.CosmeticId);

        return MapToResponse(cosmetic);
    }

    public async Task<CosmeticResponse?> UpdateAsync(int id, UpdateCosmeticRequest request, int userId)
    {
        var cosmetics = await _cosmeticRepo.FindAsync(c => c.CosmeticId == id);
        var cosmetic = cosmetics.FirstOrDefault();
        if (cosmetic == null) return null;

        cosmetic.ProductName = request.ProductName;
        cosmetic.Description = request.Description;
        cosmetic.CosmeticCategoryId = request.CosmeticCategoryId;
        cosmetic.UnitTypeId = request.UnitTypeId;
        cosmetic.Price = request.Price;
        if (request.IsActive.HasValue)
            cosmetic.IsActive = request.IsActive.Value;

        await _cosmeticRepo.UpdateAsync(cosmetic);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Updated cosmetic: {cosmetic.ProductName}", "Cosmetics", cosmetic.CosmeticId);

        return MapToResponse(cosmetic);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var cosmetics = await _cosmeticRepo.FindAsync(c => c.CosmeticId == id);
        var cosmetic = cosmetics.FirstOrDefault();
        if (cosmetic == null) return;

        cosmetic.IsActive = false;
        await _cosmeticRepo.UpdateAsync(cosmetic);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Deactivated cosmetic: {cosmetic.ProductName}", "Cosmetics", cosmetic.CosmeticId);
    }

    public async Task<CosmeticResponse> AddBatchAsync(AddBatchRequest request, int userId)
    {
        var cosmetics = await _cosmeticRepo.FindAsync(c => c.CosmeticId == request.CosmeticId);
        var cosmetic = cosmetics.Include(c => c.Batches).FirstOrDefault();
        if (cosmetic == null) throw new Exception("Cosmetic not found");

        var batch = new CosmeticBatch
        {
            CosmeticId = request.CosmeticId,
            BatchNumber = request.BatchNumber,
            QuantityReceived = request.Quantity,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            ExpiryDate = request.ExpiryDate,
            DateReceived = DateTime.Now,
            Remarks = string.Empty
        };

        await _batchRepo.AddAsync(batch);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Added batch {request.BatchNumber} to cosmetic: {cosmetic.ProductName}", "CosmeticBatches", batch.BatchId);

        return MapToResponse(cosmetic);
    }

    private static CosmeticResponse MapToResponse(Cosmetic c)
    {
        return new CosmeticResponse
        {
            CosmeticId = c.CosmeticId,
            ProductName = c.ProductName,
            Description = c.Description,
            CosmeticCategoryId = c.CosmeticCategoryId,
            CosmeticCategoryName = c.CosmeticCategory != null ? c.CosmeticCategory.Name : "",
            UnitTypeId = c.UnitTypeId,
            UnitTypeName = c.UnitType != null ? c.UnitType.Name : "",
            Price = c.Price,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            Batches = c.Batches.Select(b => new BatchResponse
            {
                BatchId = b.BatchId,
                CosmeticId = b.CosmeticId,
                BatchNumber = b.BatchNumber,
                QuantityReceived = b.QuantityReceived,
                QuantityIssued = b.QuantityIssued,
                QuantityDamaged = b.QuantityDamaged,
                QuantityExpired = b.QuantityExpired,
                Balance = b.Balance,
                ExpiryDate = b.ExpiryDate,
                DateReceived = b.DateReceived
            }).ToList()
        };
    }
}
