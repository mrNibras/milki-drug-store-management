using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.DTOs.Cosmetic;
using MilkiDrugStore.Application.Interfaces;
using MediatR;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CosmeticsController : ControllerBase
{
    private readonly ICosmeticService _cosmeticService;
    private readonly IMediator _mediator;

    public CosmeticsController(ICosmeticService cosmeticService, IMediator mediator)
    {
        _cosmeticService = cosmeticService;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? categoryId, [FromQuery] int? branchId)
    {
        var branchIdClaim = User.FindFirst("branchId")?.Value;
        int? userBranchId = int.TryParse(branchIdClaim, out var b) && b > 0 ? b : null;
        var effectiveBranchId = branchId.HasValue ? branchId : userBranchId;
        var result = await _mediator.Send(new Application.Queries.Cosmetics.GetCosmeticsQuery(search, categoryId, effectiveBranchId));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new Application.Queries.Cosmetics.GetCosmeticByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCosmeticRequest request)
    {
        try
        {
            var userId = GetUserId();
            var branchIdClaim = User.FindFirst("branchId")?.Value;
            if (request.BranchId == null || request.BranchId == 0)
            {
                if (int.TryParse(branchIdClaim, out var claimBranchId) && claimBranchId > 0)
                    request.BranchId = claimBranchId;
            }
            var result = await _mediator.Send(new Application.Commands.Cosmetics.CreateCosmeticCommand(request, userId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCosmeticRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new Application.Commands.Cosmetics.UpdateCosmeticCommand(id, request, userId));
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new Application.Commands.Cosmetics.DeleteCosmeticCommand(id, userId));
            if (!result) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("batches")]
    public async Task<IActionResult> AddBatch([FromBody] AddBatchRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new Application.Commands.Cosmetics.AddBatchCommand(request, userId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
