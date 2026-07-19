using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.UnitType;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitTypesController : ControllerBase
{
    private readonly IRepository<UnitType> _unitTypeRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;

    public UnitTypesController(IRepository<UnitType> unitTypeRepo, IUnitOfWork unitOfWork, IAuditLogService auditLog)
    {
        _unitTypeRepo = unitTypeRepo;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var unitTypes = (await _unitTypeRepo.GetAllAsync()).ToList();
        var result = unitTypes.Select(u => new UnitTypeResponse
        {
            UnitTypeId = u.UnitTypeId,
            Name = u.Name,
            Description = u.Description,
            IsActive = u.IsActive
        });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] UnitType unitType)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;

        unitType.UnitTypeId = 0;
        await _unitTypeRepo.AddAsync(unitType);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created unit type: {unitType.Name}", "UnitTypes", unitType.UnitTypeId);

        var result = new UnitTypeResponse
        {
            UnitTypeId = unitType.UnitTypeId,
            Name = unitType.Name,
            Description = unitType.Description,
            IsActive = unitType.IsActive
        };
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UnitType unitType)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;

        var existing = (await _unitTypeRepo.FindAsync(u => u.UnitTypeId == id)).FirstOrDefault();
        if (existing == null) return NotFound();

        existing.Name = unitType.Name;
        existing.Description = unitType.Description;
        existing.IsActive = unitType.IsActive;

        await _unitTypeRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Updated unit type: {existing.Name}", "UnitTypes", existing.UnitTypeId);

        var result = new UnitTypeResponse
        {
            UnitTypeId = existing.UnitTypeId,
            Name = existing.Name,
            Description = existing.Description,
            IsActive = existing.IsActive
        };
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;

        var existing = (await _unitTypeRepo.FindAsync(u => u.UnitTypeId == id)).FirstOrDefault();
        if (existing == null) return NotFound();

        existing.IsActive = false;
        await _unitTypeRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Deleted unit type: {existing.Name}", "UnitTypes", existing.UnitTypeId);

        return NoContent();
    }
}
