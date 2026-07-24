using MediatR;
using MilkiDrugStore.Application.Commands.Cosmetics;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Cosmetic;

namespace MilkiDrugStore.Application.CommandHandlers.Cosmetics;

public class UpdateCosmeticCommandHandler : IRequestHandler<UpdateCosmeticCommand, CosmeticResponse?>
{
    private readonly ICosmeticService _cosmeticService;

    public UpdateCosmeticCommandHandler(ICosmeticService cosmeticService)
    {
        _cosmeticService = cosmeticService;
    }

    public async Task<CosmeticResponse?> Handle(UpdateCosmeticCommand request, CancellationToken cancellationToken)
    {
        return await _cosmeticService.UpdateAsync(request.Id, request.Request, request.UserId);
    }
}
