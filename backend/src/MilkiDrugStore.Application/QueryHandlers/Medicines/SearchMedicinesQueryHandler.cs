using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Medicines;
using MilkiDrugStore.Application.DTOs.Medicine;

namespace MilkiDrugStore.Application.QueryHandlers.Medicines;

public class SearchMedicinesQueryHandler : IRequestHandler<SearchMedicinesQuery, IEnumerable<MedicineSearchResponse>>
{
    private readonly IMedicineService _medicineService;

    public SearchMedicinesQueryHandler(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    public async Task<IEnumerable<MedicineSearchResponse>> Handle(SearchMedicinesQuery request, CancellationToken cancellationToken)
    {
        return await _medicineService.SearchAsync(request.Query, request.BranchId);
    }
}