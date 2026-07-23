using MediatR;
using MilkiDrugStore.Application.Commands.Sales;
using MilkiDrugStore.Application.DTOs.Sale;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Application.CommandHandlers.Sales;

public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, SaleResponse>
{
    private readonly ISaleService _saleService;

    public CreateSaleCommandHandler(ISaleService saleService)
    {
        _saleService = saleService;
    }

    public async Task<SaleResponse> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        return await _saleService.CreateAsync(request.Request, request.UserId, request.UserRole, request.BranchId);
    }
}
