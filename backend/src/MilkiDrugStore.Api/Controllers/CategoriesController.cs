using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Application.Interfaces;

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
        var categories = (await _categoryRepo.GetAllAsync()).ToList();
        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Category category)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;

        category.CategoryId = 0;
        await _categoryRepo.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        await _auditLog.LogAsync(userId, $"Created category: {category.Name}", "Categories", category.CategoryId);

        return Ok(category);
    }
}
