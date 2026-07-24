using MilkiDrugStore.Application.Commands.Suppliers;
using MilkiDrugStore.Application.CommandHandlers.Suppliers;
using MilkiDrugStore.Application.Queries.Suppliers;
using MilkiDrugStore.Application.QueryHandlers.Suppliers;
using MilkiDrugStore.Application.DTOs.Supplier;
using MilkiDrugStore.Application.Interfaces;
using Moq;
using Xunit;

namespace MilkiDrugStore.Tests.Application.CQRS;

public class SupplierCQRSTests
{
    [Fact]
    public async Task CreateSupplierCommandHandler_Should_Call_Service()
    {
        var service = new Mock<ISupplierService>();
        var handler = new CreateSupplierCommandHandler(service.Object);
        var request = new CreateSupplierRequest { SupplierName = "Test" };
        service.Setup(s => s.CreateAsync(It.IsAny<CreateSupplierRequest>(), It.IsAny<int>()))
            .ReturnsAsync(new SupplierResponse { SupplierId = 1, SupplierName = "Test" });

        var result = await handler.Handle(new CreateSupplierCommand(request, 1), default);

        Assert.NotNull(result);
        Assert.Equal(1, result.SupplierId);
    }

    [Fact]
    public async Task GetSuppliersQueryHandler_Should_Call_Service()
    {
        var service = new Mock<ISupplierService>();
        var handler = new GetSuppliersQueryHandler(service.Object);
        service.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<SupplierResponse>());

        await handler.Handle(new GetSuppliersQuery(), default);

        service.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetSupplierByIdQueryHandler_Should_Call_Service()
    {
        var service = new Mock<ISupplierService>();
        var handler = new GetSupplierByIdQueryHandler(service.Object);
        service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new SupplierResponse { SupplierId = 1 });

        var result = await handler.Handle(new GetSupplierByIdQuery(1), default);

        Assert.NotNull(result);
        service.Verify(s => s.GetByIdAsync(1), Times.Once);
    }
}
