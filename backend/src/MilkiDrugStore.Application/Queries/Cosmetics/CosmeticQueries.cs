using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Cosmetic;

namespace MilkiDrugStore.Application.Queries.Cosmetics;

public record GetCosmeticsQuery(string? Search = null, int? CategoryId = null) : IRequest<IEnumerable<CosmeticResponse>>;
public record GetCosmeticByIdQuery(int Id) : IRequest<CosmeticResponse?>;
