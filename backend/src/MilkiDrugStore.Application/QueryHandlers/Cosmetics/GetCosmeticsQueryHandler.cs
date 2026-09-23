using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Cosmetics;
using MilkiDrugStore.Application.DTOs.Cosmetic;

namespace MilkiDrugStore.Application.QueryHandlers.Cosmetics;

public class GetCosmeticsQueryHandler : IRequestHandler<GetCosmeticsQuery, IEnumerable<CosmeticResponse>>
{
    private readonly ICosmeticService _cosmeticService;

    public GetCosmeticsQueryHandler(ICosmeticService cosmeticService)
    {
        _cosmeticService = cosmeticService;
    }

    public async Task<IEnumerable<CosmeticResponse>> Handle(GetCosmeticsQuery request, CancellationToken cancellationToken)
    {
        return await _cosmeticService.GetAllAsync(request.Search, request.CategoryId, request.BranchId);
    }
}
