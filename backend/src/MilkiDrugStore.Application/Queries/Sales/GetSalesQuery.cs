using MediatR;
using MilkiDrugStore.Application.DTOs.Sale;

namespace MilkiDrugStore.Application.Queries.Sales;

public record GetSalesQuery(int? BranchId = null) : IRequest<IEnumerable<SaleResponse>>;
