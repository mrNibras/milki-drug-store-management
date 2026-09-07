using MilkiDrugStore.Application.DTOs.Supplier;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _supplierRepo;
    private readonly IRepository<Purchase> _purchaseRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;

    public SupplierService(IRepository<Supplier> supplierRepo, IRepository<Purchase> purchaseRepo, IUnitOfWork unitOfWork, IAuditLogService auditLog)
    {
        _supplierRepo = supplierRepo;
        _purchaseRepo = purchaseRepo;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    public async Task<IEnumerable<SupplierResponse>> GetAllAsync()
    {
        var suppliers = await _supplierRepo.GetAllAsync();
        var list = await suppliers.ToListAsync();
        return list.Select(MapToResponse);
    }

    public async Task<SupplierResponse?> GetByIdAsync(int id)
    {
        var suppliers = await _supplierRepo.FindAsync(s => s.SupplierId == id);
        var supplier = suppliers.FirstOrDefault();
        if (supplier == null) return null;
        return MapToResponse(supplier);
    }

    public async Task<SupplierResponse> CreateAsync(CreateSupplierRequest request, int userId)
    {
        var supplier = new Supplier
        {
            SupplierName = request.SupplierName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            PaymentStatus = request.PaymentStatus ?? "Outstanding",
            CreatedAt = DateTime.Now
        };

        await _supplierRepo.AddAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Added supplier: {supplier.SupplierName}", "Suppliers", supplier.SupplierId);

        return MapToResponse(supplier);
    }

    public async Task<SupplierResponse?> UpdateAsync(int id, UpdateSupplierRequest request, int userId)
    {
        var suppliers = await _supplierRepo.FindAsync(s => s.SupplierId == id);
        var supplier = suppliers.FirstOrDefault();
        if (supplier == null) return null;

        supplier.SupplierName = request.SupplierName;
        supplier.Phone = request.Phone;
        supplier.Email = request.Email;
        supplier.Address = request.Address;
        supplier.PaymentStatus = request.PaymentStatus;

        await _supplierRepo.UpdateAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Updated supplier: {supplier.SupplierName}", "Suppliers", supplier.SupplierId);

        return MapToResponse(supplier);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var suppliers = await _supplierRepo.FindAsync(s => s.SupplierId == id);
        var supplier = suppliers.FirstOrDefault();
        if (supplier == null) return;

        await _supplierRepo.DeleteAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Deleted supplier: {supplier.SupplierName}", "Suppliers", supplier.SupplierId);
    }

    private static SupplierResponse MapToResponse(Supplier s)
    {
        return new SupplierResponse
        {
            SupplierId = s.SupplierId,
            SupplierName = s.SupplierName,
            Phone = s.Phone,
            Email = s.Email,
            Address = s.Address,
            PaymentStatus = s.PaymentStatus,
            CreatedAt = s.CreatedAt
        };
    }
}
