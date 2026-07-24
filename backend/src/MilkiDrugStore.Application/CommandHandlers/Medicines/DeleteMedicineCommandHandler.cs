using MediatR;
using MilkiDrugStore.Application.Commands.Medicines;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Application.CommandHandlers.Medicines;

public class DeleteMedicineCommandHandler : IRequestHandler<DeleteMedicineCommand, bool>
{
    private readonly IMedicineService _medicineService;

    public DeleteMedicineCommandHandler(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    public async Task<bool> Handle(DeleteMedicineCommand request, CancellationToken cancellationToken)
    {
        await _medicineService.DeleteAsync(request.Id, request.UserId);
        return true;
    }
}
