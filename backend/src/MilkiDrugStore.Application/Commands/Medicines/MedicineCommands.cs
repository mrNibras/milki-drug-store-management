using MediatR;
using MilkiDrugStore.Application.DTOs.Medicine;

namespace MilkiDrugStore.Application.Commands.Medicines;

public record CreateMedicineCommand(CreateMedicineRequest Request, int UserId) : IRequest<MedicineResponse>;
public record UpdateMedicineCommand(int Id, UpdateMedicineRequest Request, int UserId) : IRequest<MedicineResponse?>;
public record DeleteMedicineCommand(int Id, int UserId) : IRequest<bool>;
