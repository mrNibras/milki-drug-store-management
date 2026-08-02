using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Medicine;

namespace MilkiDrugStore.Application.Queries.Medicines;

public record GetMedicinesQuery(string? SearchTerm = null, int? CategoryId = null, int? BranchId = null) : IRequest<IEnumerable<MedicineResponse>>;
public record GetMedicineByIdQuery(int Id, int? BranchId = null) : IRequest<MedicineResponse?>;
public record SearchMedicinesQuery(string Query, int? BranchId = null) : IRequest<IEnumerable<MedicineSearchResponse>>;
