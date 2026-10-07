using MilkiDrugStore.Application.DTOs.Cosmetic;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Application.Services;

public class CosmeticService : ICosmeticService
{
    private readonly ICosmeticRepository _cosmeticRepo;
    private readonly IRepository<CosmeticBatch> _batchRepo;
    private readonly ICatalogService _catalog;
    private readonly IAuditLogService _auditLog;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CosmeticService> _logger;
    private readonly ICurrentUserService _currentUser;

    public CosmeticService(
        ICosmeticRepository cosmeticRepo,
        IRepository<CosmeticBatch> batchRepo,
        ICatalogService catalog,
        IAuditLogService auditLog,
        IUnitOfWork unitOfWork,
        ILogger<CosmeticService> logger,
        ICurrentUserService currentUser)
    {
        _cosmeticRepo = cosmeticRepo;
        _batchRepo = batchRepo;
        _catalog = catalog;
        _auditLog = auditLog;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Buying price is what the pharmacy paid the supplier, so only
    /// admins may receive it. Selling price stays available because the
    /// POS prices cart lines from it.
    /// </summary>
    private bool CanSeeSupplierCost()
        => _currentUser.CanViewFinancialCosts;

    public async Task<IEnumerable<CosmeticResponse>> GetAllAsync(string? search = null, int? categoryId = null, int? branchId = null)
    {
        var query = (await _cosmeticRepo.GetAllAsync()).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.ProductName.Contains(search) || c.Description.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(c => c.CategoryId == categoryId.Value);

        if (branchId.HasValue && branchId.Value > 0)
            query = query.Where(c => c.BranchId == branchId.Value);

        var cosmetics = query
            .Include(c => c.Batches)
                .ThenInclude(b => b.Supplier)
            .OrderBy(c => c.ProductName)
            .ToList();

        return await MapToResponsesAsync(cosmetics);
    }

    public async Task<CosmeticResponse?> GetByIdAsync(int id, int? branchId = null)
    {
        var cosmetics = await _cosmeticRepo.FindAsync(c => c.CosmeticId == id);
        var query = cosmetics.AsQueryable();
        if (branchId.HasValue && branchId.Value > 0)
            query = query.Where(c => c.BranchId == branchId.Value);
        var cosmetic = query.Include(c => c.Batches).ThenInclude(b => b.Supplier).FirstOrDefault();
        if (cosmetic == null) return null;
        return await MapToResponseAsync(cosmetic);
    }

    public async Task<CosmeticResponse> CreateAsync(CreateCosmeticRequest request, int userId)
    {
        request.BranchId ??= 0;
        return await CreateAsync(request, userId, request.BranchId);
    }

    public async Task<CosmeticResponse> CreateAsync(CreateCosmeticRequest request, int userId, int? branchId = null)
    {
        var effectiveBranchId = branchId ?? request.BranchId ?? 0;
        var categoryId = await ResolveCategoryIdAsync(request.CategoryId, request.NewCategoryName, userId);
        var unitTypeId = await ResolveUnitTypeIdAsync(request.UnitTypeId, request.NewUnitTypeName, userId);

        var cosmetic = new Cosmetic
        {
            ProductName = request.ProductName,
            Description = request.Description,
            CategoryId = categoryId,
            UnitTypeId = unitTypeId,
            BranchId = effectiveBranchId > 0 ? effectiveBranchId : request.BranchId ?? 0,
            SupplierId = request.SupplierId,
            Price = request.Price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _cosmeticRepo.AddAsync(cosmetic);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created cosmetic: {cosmetic.ProductName}", "Cosmetics", cosmetic.CosmeticId);

        return await MapToResponseAsync(cosmetic);
    }

    public async Task<CosmeticResponse?> UpdateAsync(int id, UpdateCosmeticRequest request, int userId)
    {
        var cosmetics = await _cosmeticRepo.FindAsync(c => c.CosmeticId == id);
        var cosmetic = cosmetics.FirstOrDefault();
        if (cosmetic == null) return null;

        var categoryId = await ResolveCategoryIdAsync(request.CategoryId, request.NewCategoryName, userId);
        var unitTypeId = await ResolveUnitTypeIdAsync(request.UnitTypeId, request.NewUnitTypeName, userId);

        cosmetic.ProductName = request.ProductName;
        cosmetic.Description = request.Description;
        cosmetic.CategoryId = categoryId;
        cosmetic.UnitTypeId = unitTypeId;
        cosmetic.Price = request.Price;
        if (request.IsActive.HasValue)
            cosmetic.IsActive = request.IsActive.Value;

        await _cosmeticRepo.UpdateAsync(cosmetic);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Updated cosmetic: {cosmetic.ProductName}", "Cosmetics", cosmetic.CosmeticId);

        return await MapToResponseAsync(cosmetic);
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
        var cosmetic = cosmetics.Include(c => c.Batches).ThenInclude(b => b.Supplier).FirstOrDefault();
        if (cosmetic == null) throw new Exception("Cosmetic not found");

        var batch = new CosmeticBatch
        {
            CosmeticId = request.CosmeticId,
            BatchNumber = request.BatchNumber,
            QuantityReceived = request.Quantity,
            QuantityIssued = 0,
            QuantityDamaged = 0,
            QuantityExpired = 0,
            ExpiryDate = ToUtc(request.ExpiryDate),
            BuyingPrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            LowStockThreshold = request.LowStockThreshold,
            BranchId = request.BranchId,
            SupplierId = request.SupplierId,
            DateReceived = DateTime.UtcNow,
            Remarks = string.Empty
        };

        await _batchRepo.AddAsync(batch);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Added batch {request.BatchNumber} to cosmetic: {cosmetic.ProductName}", "CosmeticBatches", batch.BatchId);

        return await MapToResponseAsync(cosmetic);
    }

    private async Task<int> ResolveCategoryIdAsync(int requested, string? newName, int userId)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            var resolved = await _catalog.ResolveCategoryIdAsync(newName, userId);
            if (resolved.HasValue)
                return resolved.Value;
        }

        if (await _catalog.IsValidCategoryIdAsync(requested))
            return requested;

        throw new Exception("A valid category is required.");
    }

    private async Task<int> ResolveUnitTypeIdAsync(int requested, string? newName, int userId)
    {
        if (!string.IsNullOrWhiteSpace(newName))
        {
            var resolved = await _catalog.ResolveUnitTypeIdAsync(newName, userId);
            if (resolved.HasValue)
                return resolved.Value;
        }

        if (await _catalog.IsValidUnitTypeIdAsync(requested))
            return requested;

        throw new Exception("A valid unit type is required.");
    }

    private async Task<List<CosmeticResponse>> MapToResponsesAsync(IEnumerable<Cosmetic> cosmetics)
    {
        var list = cosmetics.ToList();
        var categoryNames = await _catalog.GetCategoryNamesAsync(list.Select(c => c.CategoryId));
        var unitTypeNames = await _catalog.GetUnitTypeNamesAsync(list.Select(c => c.UnitTypeId));

        return list.Select(c => MapToResponse(c, categoryNames, unitTypeNames)).ToList();
    }

    private async Task<CosmeticResponse> MapToResponseAsync(Cosmetic c)
    {
        var categoryNames = await _catalog.GetCategoryNamesAsync(new[] { c.CategoryId });
        var unitTypeNames = await _catalog.GetUnitTypeNamesAsync(new[] { c.UnitTypeId });
        return MapToResponse(c, categoryNames, unitTypeNames);
    }

    private CosmeticResponse MapToResponse(Cosmetic c, IDictionary<int, string> categoryNames, IDictionary<int, string> unitTypeNames)
    {
        return new CosmeticResponse
        {
            CosmeticId = c.CosmeticId,
            ProductName = c.ProductName,
            Description = c.Description,
            CategoryId = c.CategoryId,
            CategoryName = categoryNames.TryGetValue(c.CategoryId, out var categoryName) ? categoryName : "",
            UnitTypeId = c.UnitTypeId,
            UnitTypeName = unitTypeNames.TryGetValue(c.UnitTypeId, out var unitTypeName) ? unitTypeName : "",
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
                BuyingPrice = CanSeeSupplierCost() ? b.BuyingPrice : null,
                SellingPrice = b.SellingPrice,
                LowStockThreshold = b.LowStockThreshold,
                BranchId = b.BranchId,
                SupplierId = b.SupplierId,
                SupplierName = b.Supplier?.SupplierName,
                DateReceived = b.DateReceived
            }).ToList()
        };
    }

    private static DateTime ToUtc(DateTime dt)
    {
        return dt.Kind == DateTimeKind.Utc
            ? dt
            : dt.Kind == DateTimeKind.Local
                ? dt.ToUniversalTime()
                : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    private static DateTime? ToUtc(DateTime? dt) => dt.HasValue ? ToUtc(dt.Value) : null;
}
