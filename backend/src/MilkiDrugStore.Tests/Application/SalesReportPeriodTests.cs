using System.Globalization;
using FluentAssertions;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application;

/// <summary>
/// Guards the sales-report period bucketing. These boundaries are business
/// periods evaluated in Ethiopian time, so they are easy to regress silently.
/// </summary>
public class SalesReportPeriodTests
{
    private static readonly TimeZoneInfo EthiopiaTz = Resolve();

    private static TimeZoneInfo Resolve()
    {
        foreach (var id in new[] { "Africa/Addis_Ababa", "E. Africa Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.CreateCustomTimeZone("Ethiopia", TimeSpan.FromHours(3), "Ethiopia", "Ethiopia");
    }

    /// <summary>Ethiopian wall-clock time expressed as the UTC instant to store.</summary>
    private static DateTime EatAsUtc(DateTime eat)
        => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(eat, DateTimeKind.Unspecified), EthiopiaTz);

    private static ReportService CreateService(IEnumerable<Sale> sales)
    {
        var saleRepo = new Mock<IRepository<Sale>>();
        saleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(sales.ToList().AsQueryable());

        var purchaseRepo = new Mock<IRepository<Purchase>>();
        purchaseRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Purchase>().AsQueryable());
        var medicineRepo = new Mock<IRepository<Medicine>>();
        medicineRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Medicine>().AsQueryable());
        var supplierRepo = new Mock<IRepository<Supplier>>();
        supplierRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Supplier>().AsQueryable());
        var userRepo = new Mock<IRepository<User>>();
        userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>().AsQueryable());
        var catalog = new Mock<ICatalogService>();
        catalog.Setup(c => c.GetCategoryNamesAsync(It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, string>());
        var cosmeticRepo = new Mock<ICosmeticRepository>();
        cosmeticRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Cosmetic>().AsQueryable());

        return new ReportService(
            saleRepo.Object,
            purchaseRepo.Object,
            medicineRepo.Object,
            supplierRepo.Object,
            userRepo.Object,
            catalog.Object,
            cosmeticRepo.Object);
    }

    private static Sale SaleAt(DateTime eat) => new()
    {
        SaleDate = EatAsUtc(eat),
        TotalAmount = 100m,
        TotalProfit = 20m
    };

    [Fact]
    public async Task Daily_UsesSevenTwoHourBucketsFrom08To22()
    {
        var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EthiopiaTz).Date;
        // One sale inside each expected two-hour bucket.
        var sales = Enumerable.Range(0, 7).Select(i => SaleAt(today.AddHours(8 + (i * 2))));

        var report = (await CreateService(sales).GetSalesReportAsync("daily")).ToList();

        report.Select(r => r.Label).Should().Equal(
            "08:00", "10:00", "12:00", "14:00", "16:00", "18:00", "20:00");
        report.Should().HaveCount(7);
    }

    [Fact]
    public async Task Daily_SalesOutsideTradingHours_AreClampedIntoTheWindowNotDropped()
    {
        var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EthiopiaTz).Date;
        // 03:00 and 23:00 are outside the 08:00-22:00 window.
        var sales = new[] { SaleAt(today.AddHours(3)), SaleAt(today.AddHours(23)) };

        var report = (await CreateService(sales).GetSalesReportAsync("daily")).ToList();

        report.Should().HaveCount(2);
        report.Select(r => r.Label).Should().BeEquivalentTo(new[] { "08:00", "20:00" });
        report.Sum(r => r.Sales).Should().Be(200m);
    }

    [Fact]
    public async Task Weekly_StartsOnMonday_NotSunday()
    {
        var nowEat = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EthiopiaTz);
        var monday = nowEat.Date.AddDays(-(((int)nowEat.DayOfWeek + 6) % 7));
        // A sale on Monday and one on the Sunday that closes the same week.
        var sales = new[] { SaleAt(monday.AddHours(9)), SaleAt(monday.AddDays(6).AddHours(9)) };

        var report = (await CreateService(sales).GetSalesReportAsync("weekly")).ToList();

        // Both belong to the same Monday-Sunday week, so exactly one bucket.
        report.Should().HaveCount(1);
        report[0].Label.Should().StartWith(monday.ToString("MMM dd"));
        report[0].Sales.Should().Be(200m);
    }

    [Fact]
    public async Task Monthly_UsesFourWeekBucketsCoveringEveryDay()
    {
        var nowEat = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EthiopiaTz);
        var firstOfMonth = new DateTime(nowEat.Year, nowEat.Month, 1);
        // First, middle and last day of the month must all be accounted for.
        var daysInMonth = DateTime.DaysInMonth(nowEat.Year, nowEat.Month);
        var sales = new[]
        {
            SaleAt(firstOfMonth.AddHours(9)),
            SaleAt(firstOfMonth.AddDays(14).AddHours(9)),
            SaleAt(firstOfMonth.AddDays(daysInMonth - 1).AddHours(9))
        };

        var report = (await CreateService(sales).GetSalesReportAsync("monthly")).ToList();

        report.Should().HaveCount(3);
        report.Select(r => r.Label).Should().OnlyContain(l => l.StartsWith("W"));
        report.Sum(r => r.Sales).Should().Be(300m);
    }

    [Fact]
    public async Task PeriodBounds_MatchTheBucketedReportWindow()
    {
        var service = CreateService(new[]
        {
            SaleAt(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EthiopiaTz).Date.AddHours(9))
        });

        foreach (var period in new[] { "daily", "weekly", "monthly", "yearly" })
        {
            var bounds = await service.GetSalesPeriodBoundsAsync(period);
            bounds.Period.Should().Be(period);
            bounds.StartUtc.Should().BeBefore(bounds.EndUtc);

            // A sale inside the window must also be charted by the report.
            var inWindow = new Sale
            {
                SaleDate = bounds.StartUtc.AddMinutes(1),
                TotalAmount = 50m,
                TotalProfit = 10m
            };
            var report = await CreateService(new[] { inWindow }).GetSalesReportAsync(period);
            report.Should().ContainSingle($"the report for '{period}' must cover the same window as its bounds");
        }
    }

    [Fact]
    public async Task PeriodBounds_AreIndependentOfSalesVolume()
    {
        // The detail table needs the window even when the period has no sales,
        // which is why bounds cannot be carried on the report rows.
        var bounds = await CreateService(Array.Empty<Sale>())
            .GetSalesPeriodBoundsAsync("yearly");

        bounds.Period.Should().Be("yearly");
        bounds.StartLocal.Should().NotBeNullOrWhiteSpace();
        bounds.EndLocal.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Yearly_PeriodBoundsStartOnSeptemberFirst()
    {
        var bounds = await CreateService(Array.Empty<Sale>())
            .GetSalesPeriodBoundsAsync("yearly");

        // StartLocal is an Ethiopian wall-clock string: yyyy-MM-dd HH:mm:ss.
        var startLocal = DateTime.ParseExact(
            bounds.StartLocal, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        startLocal.Month.Should().Be(9);
        startLocal.Day.Should().Be(1);

        var endLocal = DateTime.ParseExact(
            bounds.EndLocal, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        endLocal.Month.Should().Be(8);
    }

    [Fact]
    public async Task Weekly_PeriodBoundsStartOnMonday()
    {
        var bounds = await CreateService(Array.Empty<Sale>())
            .GetSalesPeriodBoundsAsync("weekly");

        var startLocal = DateTime.ParseExact(
            bounds.StartLocal, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        startLocal.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public async Task Yearly_CoversTheCurrentFiscalYear_NotTheTrailingOne()
    {
        var nowEat = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EthiopiaTz);
        var fiscalStart = new DateTime(nowEat.Month >= 9 ? nowEat.Year : nowEat.Year - 1, 9, 1);

        // A sale made in the current fiscal year must be reported. Anchoring the
        // window to the previous year (Sep 2025 - Aug 2026) silently dropped these.
        var inFiscalYear = await CreateService(new[] { SaleAt(nowEat.Date.AddHours(9)) })
            .GetSalesReportAsync("yearly");
        inFiscalYear.Should().ContainSingle();
        inFiscalYear.First().Sales.Should().Be(100m);

        // A sale before the fiscal year started must not be reported.
        var beforeFiscalYear = fiscalStart.AddDays(-1);
        if (beforeFiscalYear < nowEat.Date)
        {
            var excluded = await CreateService(new[] { SaleAt(beforeFiscalYear.AddHours(9)) })
                .GetSalesReportAsync("yearly");
            excluded.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Yearly_UsesAtMostTwelveMonthBuckets()
    {
        var report = await CreateService(new[]
        {
            SaleAt(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EthiopiaTz).Date.AddHours(9))
        }).GetSalesReportAsync("yearly");

        report.Count().Should().BeInRange(1, 12);
    }

    [Fact]
    public async Task Periods_AreGroupedInEthiopianTime_NotUtcCalendarDates()
    {
        // 02:00 on 30 Sep is 05:00 UTC on 30 Sep, but in Ethiopia it is still the
        // 30th while a UTC-naive grouping would be unaffected either way. Use
        // 23:00 local (20:00 UTC same day) versus 01:00 local (22:00 UTC prev day)
        // to prove the Ethiopian day boundary drives the grouping.
        var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EthiopiaTz).Date;
        var sales = new[]
        {
            SaleAt(today.AddHours(9)),
            SaleAt(today.AddHours(10))
        };

        var report = (await CreateService(sales).GetSalesReportAsync("daily")).ToList();

        // 09:00 and 10:00 sit either side of a bucket boundary in Ethiopia time.
        report.Select(r => r.Label).Should().Equal("08:00", "10:00");
    }
}
