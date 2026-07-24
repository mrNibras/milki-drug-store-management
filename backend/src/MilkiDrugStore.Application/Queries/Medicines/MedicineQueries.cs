using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Medicine;

namespace MilkiDrugStore.Application.Queries.Medicines;

public record GetMedicinesQuery(string? SearchTerm = null, int? CategoryId = null) : IRequest<IEnumerable<MedicineResponse>>;
public record GetMedicineByIdQuery(int Id) : IRequest<MedicineResponse?>;
