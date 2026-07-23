using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Sales;
using MilkiDrugStore.Application.DTOs.Sale;

namespace MilkiDrugStore.Application.QueryHandlers.Sales;

public class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, IEnumerable<SaleResponse>>
{
    private readonly ISaleService _saleService;

    public GetSalesQueryHandler(ISaleService saleService)
    {
        _saleService = saleService;
    }

    public async Task<IEnumerable<SaleResponse>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        return await _saleService.GetAllAsync(request.BranchId);
    }
}
