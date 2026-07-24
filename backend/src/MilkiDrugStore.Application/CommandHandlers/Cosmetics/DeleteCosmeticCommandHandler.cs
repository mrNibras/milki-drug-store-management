using MediatR;
using MilkiDrugStore.Application.Commands.Cosmetics;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Application.CommandHandlers.Cosmetics;

public class DeleteCosmeticCommandHandler : IRequestHandler<DeleteCosmeticCommand, bool>
{
    private readonly ICosmeticService _cosmeticService;

    public DeleteCosmeticCommandHandler(ICosmeticService cosmeticService)
    {
        _cosmeticService = cosmeticService;
    }

    public async Task<bool> Handle(DeleteCosmeticCommand request, CancellationToken cancellationToken)
    {
        await _cosmeticService.DeleteAsync(request.Id, request.UserId);
        return true;
    }
}
