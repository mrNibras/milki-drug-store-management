using MediatR;
using MilkiDrugStore.Application.Commands.Suppliers;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Supplier;

namespace MilkiDrugStore.Application.CommandHandlers.Suppliers;

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierResponse>
{
    private readonly ISupplierService _supplierService;

    public CreateSupplierCommandHandler(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    public async Task<SupplierResponse> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        return await _supplierService.CreateAsync(request.Request, request.UserId);
    }
}
