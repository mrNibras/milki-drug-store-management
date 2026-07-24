using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Medicines;
using MilkiDrugStore.Application.DTOs.Medicine;

namespace MilkiDrugStore.Application.QueryHandlers.Medicines;

public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, IEnumerable<MedicineResponse>>
{
    private readonly IMedicineService _medicineService;

    public GetMedicinesQueryHandler(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    public async Task<IEnumerable<MedicineResponse>> Handle(GetMedicinesQuery request, CancellationToken cancellationToken)
    {
        return await _medicineService.GetAllAsync(request.SearchTerm, request.CategoryId);
    }
}
