using MediatR;
using MilkiDrugStore.Application.Commands.Medicines;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Medicine;

namespace MilkiDrugStore.Application.CommandHandlers.Medicines;

public class CreateMedicineCommandHandler : IRequestHandler<CreateMedicineCommand, MedicineResponse>
{
    private readonly IMedicineService _medicineService;

    public CreateMedicineCommandHandler(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    public async Task<MedicineResponse> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        return await _medicineService.CreateAsync(request.Request, request.UserId);
    }
}
