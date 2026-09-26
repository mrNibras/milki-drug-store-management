using System.Globalization;
using System.Runtime.InteropServices;
using MilkiDrugStore.Application.DTOs.Report;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IRepository<Sale> _saleRepo;
        private readonly IRepository<Purchase> _purchaseRepo;
        private readonly IRepository<Medicine> _medicineRepo;
        private readonly IRepository<Supplier> _supplierRepo;
        private readonly IRepository<User> _userRepo;
        private readonly ICatalogService _catalog;
        private readonly ICosmeticRepository _cosmeticRepo;

        private static TimeZoneInfo GetEthiopiaTimeZone()
        {
            var linuxId = "Africa/Addis_Ababa";
            var windowsId = "E. Africa Standard Time";
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? linuxId : windowsId);
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? windowsId : linuxId);
            }
        }

        private static readonly TimeZoneInfo EthiopiaTz = GetEthiopiaTimeZone();

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

        private static (DateTime startUtc, DateTime endUtc) GetPeriodBounds(string period, DateTime nowUtc)
        {
            var nowEat = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, EthiopiaTz);
            var startEat = period.ToLower() switch
            {
                "daily" => nowEat.Date,
                "weekly" => nowEat.Date.AddDays(-(int)nowEat.DayOfWeek),
                "monthly" => new DateTime(nowEat.Year, nowEat.Month, 1),
                "yearly" => new DateTime(nowEat.Year - 1, 9, 1),
                _ => nowEat.Date
            };
            var endEat = startEat;
            if (period.ToLower() == "daily")
            {
                endEat = startEat.AddDays(1).AddTicks(-1);
            }
            else if (period.ToLower() == "weekly")
            {
                endEat = startEat.AddDays(7).AddTicks(-1);
            }
            else if (period.ToLower() == "monthly")
            {
                endEat = startEat.AddMonths(1).AddTicks(-1);
            }
            else if (period.ToLower() == "yearly")
            {
                endEat = startEat.AddYears(1).AddTicks(-1);
            }
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startEat, EthiopiaTz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endEat, EthiopiaTz);
            return (startUtc, endUtc);
        }

        private static string GetBucketLabel(DateTime bucketKeyUtc, string period)
        {
            var bucketEat = TimeZoneInfo.ConvertTimeFromUtc(bucketKeyUtc, EthiopiaTz);
            if (period == "daily")
            {
                return bucketEat.ToString("HH:00", CultureInfo.InvariantCulture);
            }
            else if (period == "weekly")
            {
                return $"{bucketEat:MMM dd} - {bucketEat.AddDays(6):MMM dd}";
            }
            else if (period == "monthly")
            {
                return $"W{(bucketEat.Day - 1) / 7 + 1} ({bucketEat:MMM})";
            }
            else if (period == "yearly")
            {
                return bucketEat.ToString("MMM yyyy", CultureInfo.InvariantCulture);
            }
            return bucketEat.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static DateTime GetBucketKey(DateTime saleDateUtc, string period)
        {
            var saleEat = TimeZoneInfo.ConvertTimeFromUtc(saleDateUtc, EthiopiaTz);
            if (period == "daily")
            {
                var hour = saleEat.Hour;
                var bucketStartHour = hour < 8 ? 0 : (hour < 14 ? 8 : (hour < 20 ? 14 : 20));
                var bucketStartEat = saleEat.Date.AddHours(bucketStartHour);
                return TimeZoneInfo.ConvertTimeToUtc(bucketStartEat, EthiopiaTz);
            }
            else if (period == "weekly")
            {
                var weekStartEat = saleEat.Date.AddDays(-(int)saleEat.DayOfWeek);
                return TimeZoneInfo.ConvertTimeToUtc(weekStartEat, EthiopiaTz);
            }
            else if (period == "monthly")
            {
                var day = saleEat.Day;
                int weekNumber;
                if (day <= 7) weekNumber = 1;
                else if (day <= 14) weekNumber = 2;
                else if (day <= 21) weekNumber = 3;
                else weekNumber = 4;
                var weekStartEat = new DateTime(saleEat.Year, saleEat.Month, 1).AddDays((weekNumber - 1) * 7);
                return TimeZoneInfo.ConvertTimeToUtc(weekStartEat, EthiopiaTz);
            }
            else if (period == "yearly")
            {
                // Ethiopia fiscal year: Sep 1 - Aug 31
                // Group by month within the fiscal year
                var fiscalYearStart = saleEat.Year;
                if (saleEat.Month < 9) fiscalYearStart -= 1;
                var fiscalStartEat = new DateTime(fiscalYearStart, 9, 1);
                var monthsSinceStart = ((saleEat.Year - fiscalStartEat.Year) * 12) + (saleEat.Month - fiscalStartEat.Month);
                var fiscalMonthStartEat = fiscalStartEat.AddMonths(monthsSinceStart);
                return TimeZoneInfo.ConvertTimeToUtc(fiscalMonthStartEat, EthiopiaTz);
            }
            return saleEat.Date;
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

            var nowEat = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EthiopiaTz);
            var today = nowEat.Date;

            var todaySales = sales.Where(s =>
            {
                var saleEat = TimeZoneInfo.ConvertTimeFromUtc(s.SaleDate, EthiopiaTz);
                return saleEat.Date == today;
            }).Sum(s => s.TotalAmount);

            var currentMonthEat = new DateTime(nowEat.Year, nowEat.Month, 1);
            var monthlySales = sales.Where(s =>
            {
                var saleEat = TimeZoneInfo.ConvertTimeFromUtc(s.SaleDate, EthiopiaTz);
                return saleEat >= currentMonthEat;
            }).Sum(s => s.TotalAmount);

            var monthlyProfit = sales.Where(s =>
            {
                var saleEat = TimeZoneInfo.ConvertTimeFromUtc(s.SaleDate, EthiopiaTz);
                return saleEat >= currentMonthEat;
            }).Sum(s => s.TotalProfit);

            var inventoryValue = medicines.Sum(m => m.Batches.Sum(b => b.RemainingQuantity * b.PurchasePrice));
            var cosmeticInventoryValue = cosmetics.Sum(c => c.Batches.Where(b => !branchId.HasValue || b.BranchId == branchId).Sum(b => b.Balance * b.BuyingPrice));
            var lowStockCount = medicines.Count(m => m.Batches.Sum(b => b.RemainingQuantity) <= m.ReorderLevel && m.Batches.Sum(b => b.RemainingQuantity) > 0);
            var cosmeticLowStockCount = cosmetics.Count(c => c.Batches.Where(b => !branchId.HasValue || b.BranchId == branchId.Value).Sum(b => b.Balance) <= c.Batches.Where(b => !branchId.HasValue || b.BranchId == branchId.Value).Min(b => b.LowStockThreshold) && c.Batches.Where(b => !branchId.HasValue || b.BranchId == branchId.Value).Sum(b => b.Balance) > 0);
            var expiringCount = medicines.Count(m => m.Batches.Any(b => b.ExpiryDate <= nowEat.AddMonths(6) && b.RemainingQuantity > 0));
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

            var (startUtc, endUtc) = GetPeriodBounds(period, DateTime.UtcNow);
            var filtered = sales.Where(s => s.SaleDate >= startUtc && s.SaleDate <= endUtc).ToList();

            if (filtered.Count == 0)
                return new List<SalesReportResponse>();

            return filtered
                .GroupBy(s => GetBucketKey(s.SaleDate, period.ToLower()))
                .Select(g => new SalesReportResponse
                {
                    Date = g.Key,
                    Label = GetBucketLabel(g.Key, period.ToLower()),
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
}
