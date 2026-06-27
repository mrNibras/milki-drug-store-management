using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IRepository<Category> _categoryRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(IRepository<Category> categoryRepo, IUnitOfWork unitOfWork)
    {
        _categoryRepo = categoryRepo;
        _unitOfWork = unitOfWork;
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
        await _categoryRepo.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        return Ok(category);
    }
}
