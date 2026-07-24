using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Cosmetics;
using MilkiDrugStore.Application.DTOs.Cosmetic;

namespace MilkiDrugStore.Application.QueryHandlers.Cosmetics;

public class GetCosmeticByIdQueryHandler : IRequestHandler<GetCosmeticByIdQuery, CosmeticResponse?>
{
    private readonly ICosmeticService _cosmeticService;

    public GetCosmeticByIdQueryHandler(ICosmeticService cosmeticService)
    {
        _cosmeticService = cosmeticService;
    }

    public async Task<CosmeticResponse?> Handle(GetCosmeticByIdQuery request, CancellationToken cancellationToken)
    {
        return await _cosmeticService.GetByIdAsync(request.Id);
    }
}
