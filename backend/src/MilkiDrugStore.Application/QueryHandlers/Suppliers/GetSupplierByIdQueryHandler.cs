using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Suppliers;
using MilkiDrugStore.Application.DTOs.Supplier;

namespace MilkiDrugStore.Application.QueryHandlers.Suppliers;

public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierResponse?>
{
    private readonly ISupplierService _supplierService;

    public GetSupplierByIdQueryHandler(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    public async Task<SupplierResponse?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        return await _supplierService.GetByIdAsync(request.Id);
    }
}
