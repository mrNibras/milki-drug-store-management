using MediatR;
using MilkiDrugStore.Application.DTOs.Sale;

namespace MilkiDrugStore.Application.Commands.Sales;

public record CreateSaleCommand(CreateSaleRequest Request, int UserId, string UserRole, int? BranchId = null) : IRequest<SaleResponse>;
