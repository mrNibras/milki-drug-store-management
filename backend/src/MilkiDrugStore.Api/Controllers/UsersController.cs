using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var result = await _authService.CreateUserAsync(request);
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var result = await _authService.UpdateUserAsync(id, request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _authService.DeleteUserAsync(id);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _authService.ApproveUserAsync(id);
        return Ok(new { message = result });
    }
}
