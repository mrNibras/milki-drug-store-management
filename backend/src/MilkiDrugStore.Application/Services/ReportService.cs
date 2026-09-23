using MilkiDrugStore.Application.DTOs.Report;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Application.Services;

public class ReportService : IReportService
{
    private readonly IRepository<Sale> _saleRepo;
    private readonly IRepository<Purchase> _purchaseRepo;
    private readonly IRepository<Medicine> _medicineRepo;
    private readonly IRepository<Supplier> _supplierRepo;
    private readonly IRepository<User> _userRepo;
    private readonly ICatalogService _catalog;
    private readonly ICosmeticRepository _cosmeticRepo;

    public ReportService(
        IRepository<Sale> saleRepo,
        IRepository<Purchase> purchaseRepo,
        IRepository<Medicine> medicineRepo,
        IRepository<Supplier> supplierRepo,
        IRepository<User> userRepo,
        ICatalogService catalog,
        ICosmeticRepository cosmeticRepo)
    {
        _saleRepo = saleRepo;
        _purchaseRepo = purchaseRepo;
        _medicineRepo = medicineRepo;
        _supplierRepo = supplierRepo;
        _userRepo = userRepo;
        _catalog = catalog;
        _cosmeticRepo = cosmeticRepo;
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(int? branchId = null)
    {
        var medicines = (await _medicineRepo.GetAllAsync()).ToList();
        var sales = (await _saleRepo.GetAllAsync()).ToList();
        var cosmetics = (await _cosmeticRepo.GetAllAsync()).ToList();

        if (branchId.HasValue)
        {
            sales = sales.Where(s => s.BranchId == branchId.Value).ToList();
            cosmetics = cosmetics.Where(c => c.BranchId == branchId.Value).ToList();
        }

        var today = DateTime.UtcNow.Date;

        var todaySales = sales.Where(s => s.SaleDate.Date == today).Sum(s => s.TotalAmount);
        var currentMonth = new DateTime(today.Year, today.Month, 1);
        var monthlySales = sales.Where(s => s.SaleDate >= currentMonth).Sum(s => s.TotalAmount);
        var monthlyProfit = sales.Where(s => s.SaleDate >= currentMonth).Sum(s => s.TotalProfit);

        var inventoryValue = medicines.Sum(m => m.Batches.Sum(b => b.RemainingQuantity * b.PurchasePrice));
        var cosmeticInventoryValue = cosmetics.Sum(c => c.Batches.Where(b => !branchId.HasValue || b.BranchId == branchId).Sum(b => b.Balance * b.BuyingPrice));
        var lowStockCount = medicines.Count(m => m.Batches.Sum(b => b.RemainingQuantity) <= m.ReorderLevel && m.Batches.Sum(b => b.RemainingQuantity) > 0);
        var cosmeticLowStockCount = cosmetics.Count(c => c.Batches.Where(b => !branchId.HasValue || b.BranchId == branchId.Value).Sum(b => b.Balance) <= c.Batches.Where(b => !branchId.HasValue || b.BranchId == branchId.Value).Min(b => b.LowStockThreshold) && c.Batches.Where(b => !branchId.HasValue || b.BranchId == branchId.Value).Sum(b => b.Balance) > 0);
        var expiringCount = medicines.Count(m => m.Batches.Any(b => b.ExpiryDate <= DateTime.UtcNow.AddMonths(6) && b.RemainingQuantity > 0));
        var outOfStockCount = medicines.Count(m => m.Batches.All(b => b.RemainingQuantity <= 0));

        return new DashboardSummaryResponse
        {
            TotalMedicines = medicines.Count(m => m.IsActive),
            InventoryValue = inventoryValue + cosmeticInventoryValue,
            TodaySales = todaySales,
            MonthlySales = monthlySales,
            MonthlyProfit = monthlyProfit,
            LowStockCount = lowStockCount + cosmeticLowStockCount,
            ExpiringCount = expiringCount,
            OutOfStockCount = outOfStockCount
        };
    }

    public async Task<IEnumerable<SalesReportResponse>> GetSalesReportAsync(string period, int? branchId = null)
    {
        var sales = (await _saleRepo.GetAllAsync()).ToList();
        if (branchId.HasValue)
            sales = sales.Where(s => s.BranchId == branchId.Value).ToList();
        DateTime from;

        from = period.ToLower() switch
        {
            "daily" => DateTime.UtcNow.Date,
            "weekly" => DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek),
            "monthly" => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
            "yearly" => new DateTime(DateTime.UtcNow.Year, 1, 1),
            _ => DateTime.MinValue
        };

        var filtered = sales.Where(s => s.SaleDate >= from);

        return filtered
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new SalesReportResponse
            {
                Date = g.Key,
                Sales = g.Sum(s => s.TotalAmount),
                Profit = g.Sum(s => s.TotalProfit),
                TransactionCount = g.Count()
            })
            .OrderBy(r => r.Date)
            .ToList();
    }

    public async Task<IEnumerable<InventoryReportResponse>> GetInventoryReportAsync(int? branchId = null)
    {
        var medicines = (await _medicineRepo.GetAllAsync()).ToList();
        var cosmetics = (await _cosmeticRepo.GetAllAsync())
            .Include(c => c.Batches)
            .ToList();
        var categoryNames = await _catalog.GetCategoryNamesAsync(medicines.Select(m => m.CategoryId));

        var medicineResults = medicines.Select(m =>
        {
            var batches = m.Batches.AsQueryable();
            if (branchId.HasValue)
                batches = batches.Where(b => b.BranchId == branchId.Value);
            var totalQty = batches.Sum(b => b.RemainingQuantity);
            var totalValue = batches.Sum(b => b.RemainingQuantity * b.PurchasePrice);
            var status = totalQty == 0 ? "Out of Stock" : totalQty <= m.ReorderLevel ? "Low Stock" : "In Stock";

            return new InventoryReportResponse
            {
                ProductId = m.ProductId,
                ProductCode = m.ProductCode,
                BrandName = m.BrandName,
                CategoryName = categoryNames.TryGetValue(m.CategoryId, out var name) ? name : "",
                Quantity = totalQty,
                Value = totalValue,
                Status = status
            };
        });

        var cosmeticResults = cosmetics.Select(c =>
        {
            var batches = c.Batches.AsQueryable();
            if (branchId.HasValue)
                batches = batches.Where(b => b.BranchId == branchId.Value);
            var totalQty = batches.Sum(b => b.Balance);
            var totalValue = batches.Sum(b => b.Balance * b.BuyingPrice);
            var threshold = batches.Any() ? batches.Min(b => b.LowStockThreshold) : 0;
            var status = totalQty == 0 ? "Out of Stock" : totalQty <= threshold ? "Low Stock" : "In Stock";

            return new InventoryReportResponse
            {
                ProductId = c.CosmeticId,
                ProductCode = c.CosmeticId.ToString(),
                BrandName = c.ProductName,
                ProductType = "cosmetic",
                CategoryName = CosmeticCatalog.GetCategoryName(c.CategoryId) ?? "",
                Quantity = totalQty,
                Value = totalValue,
                Status = status
            };
        });

        return medicineResults.Concat(cosmeticResults).OrderBy(r => r.Quantity).ToList();
    }

    public async Task<IEnumerable<SupplierReportResponse>> GetSupplierReportAsync(int? branchId = null)
    {
        var purchases = (await _purchaseRepo.GetAllAsync()).ToList();
        var suppliers = (await _supplierRepo.GetAllAsync()).ToList();

        if (branchId.HasValue)
            purchases = purchases.Where(p => p.BranchId == branchId.Value).ToList();

        return suppliers.Select(s =>
        {
            var supplierPurchases = purchases.Where(p => p.SupplierId == s.SupplierId).ToList();
            var totalAmount = supplierPurchases.Sum(p => p.TotalAmount);
            var totalPaid = supplierPurchases.Sum(p => p.AmountPaid);
            var totalDebt = supplierPurchases.Sum(p => p.AmountDue);

            return new SupplierReportResponse
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                Purchases = supplierPurchases.Count,
                TotalAmount = totalAmount,
                TotalPaid = totalPaid,
                TotalDebt = totalDebt,
                PaymentStatus = totalDebt > 0 ? "Outstanding" : "Cleared"
            };
        }).OrderByDescending(r => r.TotalAmount).ToList();
    }

    public async Task<IEnumerable<StaffReportResponse>> GetStaffReportAsync(int? branchId = null)
    {
        var sales = (await _saleRepo.GetAllAsync()).ToList();
        var users = (await _userRepo.GetAllAsync()).ToList();

        if (branchId.HasValue)
            sales = sales.Where(s => s.BranchId == branchId.Value).ToList();

        return users.Where(u => u.IsActive).Select(u =>
        {
            var userSales = sales.Where(s => s.UserId == u.UserId).ToList();
            return new StaffReportResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Role = u.Role != null ? u.Role.Name : "",
                SalesCount = userSales.Count,
                TotalAmount = userSales.Sum(s => s.TotalAmount),
                Profit = userSales.Sum(s => s.TotalProfit)
            };
        }).OrderByDescending(r => r.TotalAmount).ToList();
    }
}
