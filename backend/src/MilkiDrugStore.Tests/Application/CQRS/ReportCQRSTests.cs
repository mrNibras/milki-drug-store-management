using MilkiDrugStore.Application.Queries.Reports;
using MilkiDrugStore.Application.QueryHandlers.Reports;
using MilkiDrugStore.Application.DTOs.Report;
using MilkiDrugStore.Application.Interfaces;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application.CQRS;

public class ReportCQRSTests
{
    [Fact]
    public async Task GetDashboardSummaryQueryHandler_Should_Call_Service()
    {
        var service = new Mock<IReportService>();
        var handler = new GetDashboardSummaryQueryHandler(service.Object);
        service.Setup(s => s.GetDashboardSummaryAsync(It.IsAny<int?>()))
            .ReturnsAsync(new DashboardSummaryResponse { TodaySales = 1000 });

        var result = await handler.Handle(new GetDashboardSummaryQuery(1), default);

        Assert.NotNull(result);
        service.Verify(s => s.GetDashboardSummaryAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetSalesReportQueryHandler_Should_Call_Service()
    {
        var service = new Mock<IReportService>();
        var handler = new GetSalesReportQueryHandler(service.Object);
        service.Setup(s => s.GetSalesReportAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<SalesReportResponse>());

        var result = await handler.Handle(new GetSalesReportQuery("daily", 1), default);

        Assert.Empty(result);
        service.Verify(s => s.GetSalesReportAsync("daily", 1), Times.Once);
    }

    [Fact]
    public async Task GetInventoryReportQueryHandler_Should_Call_Service()
    {
        var service = new Mock<IReportService>();
        var handler = new GetInventoryReportQueryHandler(service.Object);
        service.Setup(s => s.GetInventoryReportAsync(It.IsAny<int?>()))
            .ReturnsAsync(new List<InventoryReportResponse>());

        var result = await handler.Handle(new GetInventoryReportQuery(1), default);

        Assert.Empty(result);
        service.Verify(s => s.GetInventoryReportAsync(1), Times.Once);
    }
}
