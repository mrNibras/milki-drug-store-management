using MediatR;
using MilkiDrugStore.Application.DTOs.Purchase;

namespace MilkiDrugStore.Application.Commands.Purchases;

public record CreatePurchaseCommand(CreatePurchaseRequest Request, int UserId) : IRequest<PurchaseResponse>;
