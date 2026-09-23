using MilkiDrugStore.Application.Commands.Cosmetics;
using MilkiDrugStore.Application.CommandHandlers.Cosmetics;
using MilkiDrugStore.Application.Queries.Cosmetics;
using MilkiDrugStore.Application.QueryHandlers.Cosmetics;
using MilkiDrugStore.Application.DTOs.Cosmetic;
using MilkiDrugStore.Application.Interfaces;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application.CQRS;

public class CosmeticCQRSTests
{
    [Fact]
    public async Task CreateCosmeticCommandHandler_Should_Call_Service()
    {
        var service = new Mock<ICosmeticService>();
        var handler = new CreateCosmeticCommandHandler(service.Object);
        var request = new CreateCosmeticRequest { ProductName = "Test" };
        service.Setup(s => s.CreateAsync(It.IsAny<CreateCosmeticRequest>(), It.IsAny<int>()))
            .ReturnsAsync(new CosmeticResponse { CosmeticId = 1, ProductName = "Test" });

        var result = await handler.Handle(new CreateCosmeticCommand(request, 1), default);

        Assert.NotNull(result);
        Assert.Equal(1, result.CosmeticId);
    }

    [Fact]
    public async Task GetCosmeticsQueryHandler_Should_Call_Service()
    {
        var service = new Mock<ICosmeticService>();
        var handler = new GetCosmeticsQueryHandler(service.Object);
        service.Setup(s => s.GetAllAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<CosmeticResponse>());

        var result = await handler.Handle(new GetCosmeticsQuery("search", 1), default);

        Assert.Empty(result);
        service.Verify(s => s.GetAllAsync("search", 1, null), Times.Once);
    }

    [Fact]
    public async Task AddBatchCommandHandler_Should_Call_Service()
    {
        var service = new Mock<ICosmeticService>();
        var handler = new AddBatchCommandHandler(service.Object);
        var request = new AddBatchRequest { BatchNumber = "B1" };
        service.Setup(s => s.AddBatchAsync(It.IsAny<AddBatchRequest>(), It.IsAny<int>()))
            .ReturnsAsync(new CosmeticResponse { CosmeticId = 1 });

        var result = await handler.Handle(new AddBatchCommand(request, 1), default);

        Assert.NotNull(result);
        service.Verify(s => s.AddBatchAsync(request, 1), Times.Once);
    }
}
