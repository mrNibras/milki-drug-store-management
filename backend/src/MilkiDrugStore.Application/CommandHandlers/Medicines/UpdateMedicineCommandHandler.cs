using MediatR;
using MilkiDrugStore.Application.Commands.Medicines;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Medicine;

namespace MilkiDrugStore.Application.CommandHandlers.Medicines;

public class UpdateMedicineCommandHandler : IRequestHandler<UpdateMedicineCommand, MedicineResponse?>
{
    private readonly IMedicineService _medicineService;

    public UpdateMedicineCommandHandler(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    public async Task<MedicineResponse?> Handle(UpdateMedicineCommand request, CancellationToken cancellationToken)
    {
        return await _medicineService.UpdateAsync(request.Id, request.Request, request.UserId);
    }
}
