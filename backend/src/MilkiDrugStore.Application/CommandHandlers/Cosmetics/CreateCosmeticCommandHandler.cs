using MediatR;
using MilkiDrugStore.Application.Commands.Cosmetics;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Cosmetic;

namespace MilkiDrugStore.Application.CommandHandlers.Cosmetics;

public class CreateCosmeticCommandHandler : IRequestHandler<CreateCosmeticCommand, CosmeticResponse>
{
    private readonly ICosmeticService _cosmeticService;

    public CreateCosmeticCommandHandler(ICosmeticService cosmeticService)
    {
        _cosmeticService = cosmeticService;
    }

    public async Task<CosmeticResponse> Handle(CreateCosmeticCommand request, CancellationToken cancellationToken)
    {
        return await _cosmeticService.CreateAsync(request.Request, request.UserId);
    }
}
