using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branchId = GetBranchId();
        var result = await _notificationService.GetAllAsync(branchId > 0 ? branchId : null);
        return Ok(result);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var branchId = GetBranchId();
        var result = await _notificationService.GetUnreadAsync(branchId > 0 ? branchId : null);
        return Ok(result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var branchId = GetBranchId();
        await _notificationService.MarkAllAsReadAsync(branchId > 0 ? branchId : null);
        return NoContent();
    }

    private int GetBranchId()
    {
        var branchIdClaim = User.FindFirst("branchId")?.Value;
        return int.TryParse(branchIdClaim, out var branchId) ? branchId : 0;
    }
}
