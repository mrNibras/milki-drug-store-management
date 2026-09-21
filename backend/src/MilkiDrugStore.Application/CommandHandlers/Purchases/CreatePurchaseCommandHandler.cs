using MediatR;
using MilkiDrugStore.Application.Commands.Purchases;
using MilkiDrugStore.Application.DTOs.Purchase;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Application.CommandHandlers.Purchases;

public class CreatePurchaseCommandHandler : IRequestHandler<CreatePurchaseCommand, PurchaseResponse>
{
    private readonly IPurchaseService _purchaseService;

    public CreatePurchaseCommandHandler(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

        public async Task<PurchaseResponse> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
        {
            return await _purchaseService.CreateAsync(request.Request, request.UserId, request.BranchId);
        }
}
