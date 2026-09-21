using MilkiDrugStore.Application.DTOs.Branch;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Application.Services;

public class BranchService : IBranchService
{
    private readonly IRepository<Branch> _branchRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;

    public BranchService(IRepository<Branch> branchRepo, IUnitOfWork unitOfWork, IAuditLogService auditLog)
    {
        _branchRepo = branchRepo;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    public async Task<IEnumerable<BranchResponse>> GetAllAsync()
    {
        var branches = await _branchRepo.GetAllAsync();
        var list = await branches.OrderBy(b => b.BranchName).ToListAsync();
        return list.Select(MapToResponse);
    }

    public async Task<BranchResponse?> GetByIdAsync(int id)
    {
        var branch = await _branchRepo.GetByIdAsync(id);
        if (branch == null) return null;
        return MapToResponse(branch);
    }

    public async Task<BranchResponse> CreateAsync(CreateBranchRequest request, int createdBy)
    {
        var branch = new Branch
        {
            BranchName = request.BranchName,
            Location = request.Location,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _branchRepo.AddAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(createdBy, $"Created branch: {branch.BranchName}", "Branches", branch.BranchId);

        return MapToResponse(branch);
    }

    public async Task<BranchResponse?> UpdateAsync(int id, UpdateBranchRequest request, int updatedBy)
    {
        var branches = await _branchRepo.FindAsync(b => b.BranchId == id);
        var branch = branches.FirstOrDefault();
        if (branch == null) return null;

        branch.BranchName = request.BranchName;
        branch.Location = request.Location;
        branch.Phone = request.Phone;
        branch.Email = request.Email;
        branch.Address = request.Address;
        branch.IsActive = request.IsActive;

        await _branchRepo.UpdateAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(updatedBy, $"Updated branch: {branch.BranchName}", "Branches", branch.BranchId);

        return MapToResponse(branch);
    }

    public async Task DeleteAsync(int id, int deletedBy)
    {
        var branches = await _branchRepo.FindAsync(b => b.BranchId == id);
        var branch = branches.FirstOrDefault();
        if (branch == null) return;

        branch.IsActive = false;
        await _branchRepo.UpdateAsync(branch);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(deletedBy, $"Deactivated branch: {branch.BranchName}", "Branches", branch.BranchId);
    }

    private static BranchResponse MapToResponse(Branch b)
    {
        return new BranchResponse
        {
            BranchId = b.BranchId,
            BranchName = b.BranchName,
            Location = b.Location,
            Phone = b.Phone,
            Email = b.Email,
            Address = b.Address,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt
        };
    }
}
