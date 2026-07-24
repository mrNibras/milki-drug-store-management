using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Purchases;
using MilkiDrugStore.Application.DTOs.Purchase;

namespace MilkiDrugStore.Application.QueryHandlers.Purchases;

public class GetPurchasesQueryHandler : IRequestHandler<GetPurchasesQuery, IEnumerable<PurchaseResponse>>
{
    private readonly IPurchaseService _purchaseService;

    public GetPurchasesQueryHandler(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    public async Task<IEnumerable<PurchaseResponse>> Handle(GetPurchasesQuery request, CancellationToken cancellationToken)
    {
        return await _purchaseService.GetAllAsync(request.SupplierId);
    }
}
