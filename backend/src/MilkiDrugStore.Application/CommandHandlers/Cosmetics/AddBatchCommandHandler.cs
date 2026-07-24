using MediatR;
using MilkiDrugStore.Application.Commands.Cosmetics;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Cosmetic;

namespace MilkiDrugStore.Application.CommandHandlers.Cosmetics;

public class AddBatchCommandHandler : IRequestHandler<AddBatchCommand, CosmeticResponse>
{
    private readonly ICosmeticService _cosmeticService;

    public AddBatchCommandHandler(ICosmeticService cosmeticService)
    {
        _cosmeticService = cosmeticService;
    }

    public async Task<CosmeticResponse> Handle(AddBatchCommand request, CancellationToken cancellationToken)
    {
        return await _cosmeticService.AddBatchAsync(request.Request, request.UserId);
    }
}
