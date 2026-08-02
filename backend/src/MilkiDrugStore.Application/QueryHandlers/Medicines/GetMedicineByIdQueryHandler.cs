using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Medicines;
using MilkiDrugStore.Application.DTOs.Medicine;

namespace MilkiDrugStore.Application.QueryHandlers.Medicines;

public class GetMedicineByIdQueryHandler : IRequestHandler<GetMedicineByIdQuery, MedicineResponse?>
{
    private readonly IMedicineService _medicineService;

    public GetMedicineByIdQueryHandler(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    public async Task<MedicineResponse?> Handle(GetMedicineByIdQuery request, CancellationToken cancellationToken)
    {
        return await _medicineService.GetByIdAsync(request.Id, request.BranchId);
    }
}
