using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Suppliers;
using MilkiDrugStore.Application.DTOs.Supplier;

namespace MilkiDrugStore.Application.QueryHandlers.Suppliers;

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, IEnumerable<SupplierResponse>>
{
    private readonly ISupplierService _supplierService;

    public GetSuppliersQueryHandler(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    public async Task<IEnumerable<SupplierResponse>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        return await _supplierService.GetAllAsync();
    }
}
