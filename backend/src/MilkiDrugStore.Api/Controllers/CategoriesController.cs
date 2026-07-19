using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Category;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IRepository<Category> _categoryRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;

    public CategoriesController(IRepository<Category> categoryRepo, IUnitOfWork unitOfWork, IAuditLogService auditLog)
    {
        _categoryRepo = categoryRepo;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = (await _categoryRepo.GetAllAsync())
            .Include(c => c.UnitType)
            .ToList();
        var result = categories.Select(c => new CategoryResponse
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            UnitTypeId = c.UnitTypeId,
            UnitTypeName = c.UnitType?.Name ?? "",
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Category category)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;

        category.CategoryId = 0;
        await _categoryRepo.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created category: {category.Name}", "Categories", category.CategoryId);

        var result = new CategoryResponse
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            UnitTypeId = category.UnitTypeId,
            UnitTypeName = category.UnitType?.Name ?? "",
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Category category)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;

        var existing = (await _categoryRepo.FindAsync(c => c.CategoryId == id)).FirstOrDefault();
        if (existing == null) return NotFound();

        existing.Name = category.Name;
        existing.UnitTypeId = category.UnitTypeId;
        existing.IsActive = category.IsActive;

        await _categoryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Updated category: {existing.Name}", "Categories", existing.CategoryId);

        var result = new CategoryResponse
        {
            CategoryId = existing.CategoryId,
            Name = existing.Name,
            UnitTypeId = existing.UnitTypeId,
            UnitTypeName = existing.UnitType?.Name ?? "",
            IsActive = existing.IsActive,
            CreatedAt = existing.CreatedAt
        };
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;

        var existing = (await _categoryRepo.FindAsync(c => c.CategoryId == id)).FirstOrDefault();
        if (existing == null) return NotFound();

        existing.IsActive = false;
        await _categoryRepo.UpdateAsync(existing);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Deleted category: {existing.Name}", "Categories", existing.CategoryId);

        return NoContent();
    }
}
