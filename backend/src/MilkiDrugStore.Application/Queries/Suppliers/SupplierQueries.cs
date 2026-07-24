using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Supplier;

namespace MilkiDrugStore.Application.Queries.Suppliers;

public record GetSuppliersQuery : IRequest<IEnumerable<SupplierResponse>>;
public record GetSupplierByIdQuery(int Id) : IRequest<SupplierResponse?>;
