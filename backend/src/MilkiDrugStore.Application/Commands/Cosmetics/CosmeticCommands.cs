using MediatR;
using MilkiDrugStore.Application.DTOs.Cosmetic;

namespace MilkiDrugStore.Application.Commands.Cosmetics;

public record CreateCosmeticCommand(CreateCosmeticRequest Request, int UserId) : IRequest<CosmeticResponse>;
public record UpdateCosmeticCommand(int Id, UpdateCosmeticRequest Request, int UserId) : IRequest<CosmeticResponse?>;
public record DeleteCosmeticCommand(int Id, int UserId) : IRequest<bool>;
public record AddBatchCommand(AddBatchRequest Request, int UserId) : IRequest<CosmeticResponse>;
