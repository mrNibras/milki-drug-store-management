using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;
using MediatR;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly IMediator _mediator;

    public SuppliersController(ISupplierService supplierService, IMediator mediator)
    {
        _supplierService = supplierService;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new Application.Queries.Suppliers.GetSuppliersQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new Application.Queries.Suppliers.GetSupplierByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MilkiDrugStore.Application.DTOs.Supplier.CreateSupplierRequest request)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new Application.Commands.Suppliers.CreateSupplierCommand(request, userId));
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MilkiDrugStore.Application.DTOs.Supplier.UpdateSupplierRequest request)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new Application.Commands.Suppliers.UpdateSupplierCommand(id, request, userId));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new Application.Commands.Suppliers.DeleteSupplierCommand(id, userId));
        if (!result) return NotFound();
        return NoContent();
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
