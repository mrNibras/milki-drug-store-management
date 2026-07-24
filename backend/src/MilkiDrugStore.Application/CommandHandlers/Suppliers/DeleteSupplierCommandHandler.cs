using MediatR;
using MilkiDrugStore.Application.Commands.Suppliers;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Application.CommandHandlers.Suppliers;

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, bool>
{
    private readonly ISupplierService _supplierService;

    public DeleteSupplierCommandHandler(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    public async Task<bool> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        await _supplierService.DeleteAsync(request.Id, request.UserId);
        return true;
    }
}
