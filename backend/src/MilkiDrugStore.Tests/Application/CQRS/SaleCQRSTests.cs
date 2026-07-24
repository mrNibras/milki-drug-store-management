using MilkiDrugStore.Application.Commands.Sales;
using MilkiDrugStore.Application.CommandHandlers.Sales;
using MilkiDrugStore.Application.DTOs.Sale;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Sales;
using MilkiDrugStore.Application.QueryHandlers.Sales;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application.CQRS;

public class SaleCQRSTests
{
    [Fact]
    public async Task CreateSaleCommandHandler_Should_Call_SaleService()
    {
        var saleService = new Mock<ISaleService>();
        var handler = new CreateSaleCommandHandler(saleService.Object);
        var request = new CreateSaleRequest { PaymentMethod = "cash", AmountPaid = 100 };
        saleService.Setup(s => s.CreateAsync(It.IsAny<CreateSaleRequest>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(new SaleResponse { SaleId = 1 });

        var result = await handler.Handle(new CreateSaleCommand(request, 1, "Pharmacist", null), default);

        Assert.NotNull(result);
        Assert.Equal(1, result.SaleId);
        saleService.Verify(s => s.CreateAsync(request, 1, "Pharmacist", null), Times.Once);
    }

    [Fact]
    public async Task GetSalesQueryHandler_Should_Call_SaleService()
    {
        var saleService = new Mock<ISaleService>();
        var handler = new GetSalesQueryHandler(saleService.Object);
        saleService.Setup(s => s.GetAllAsync(It.IsAny<int?>()))
            .ReturnsAsync(new List<SaleResponse> { new SaleResponse { SaleId = 1 } });

        var result = await handler.Handle(new GetSalesQuery(1), default);

        Assert.Single(result);
        saleService.Verify(s => s.GetAllAsync(1), Times.Once);
    }
}
