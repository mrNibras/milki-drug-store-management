using MediatR;
using MilkiDrugStore.Application.Commands.Suppliers;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Supplier;

namespace MilkiDrugStore.Application.CommandHandlers.Suppliers;

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, SupplierResponse?>
{
    private readonly ISupplierService _supplierService;

    public UpdateSupplierCommandHandler(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    public async Task<SupplierResponse?> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        return await _supplierService.UpdateAsync(request.Id, request.Request, request.UserId);
    }
}
