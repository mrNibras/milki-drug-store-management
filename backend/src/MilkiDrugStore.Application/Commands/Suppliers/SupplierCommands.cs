using MediatR;
using MilkiDrugStore.Application.DTOs.Supplier;

namespace MilkiDrugStore.Application.Commands.Suppliers;

public record CreateSupplierCommand(CreateSupplierRequest Request, int UserId) : IRequest<SupplierResponse>;
public record UpdateSupplierCommand(int Id, UpdateSupplierRequest Request, int UserId) : IRequest<SupplierResponse?>;
public record DeleteSupplierCommand(int Id, int UserId) : IRequest<bool>;
