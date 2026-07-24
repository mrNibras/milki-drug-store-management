using MediatR;
using MilkiDrugStore.Application.DTOs.Purchase;

namespace MilkiDrugStore.Application.Queries.Purchases;

public record GetPurchasesQuery(int? SupplierId = null) : IRequest<IEnumerable<PurchaseResponse>>;
