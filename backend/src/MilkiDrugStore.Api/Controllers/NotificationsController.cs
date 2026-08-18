using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilkiDrugStore.Application.Interfaces;
using MediatR;

namespace MilkiDrugStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Pharmacist")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;

    public NotificationsController(INotificationService notificationService, IMediator mediator)
    {
        _notificationService = notificationService;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new Application.Queries.Notifications.GetNotificationsQuery());
        return Ok(result);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var result = await _mediator.Send(new Application.Queries.Notifications.GetNotificationsQuery(UnreadOnly: true));
        return Ok(result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _mediator.Send(new Application.Commands.Notifications.MarkNotificationAsReadCommand(id));
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync();
        return NoContent();
    }

    private int GetBranchId()
    {
        var branchIdClaim = User.FindFirst("branchId")?.Value;
        return int.TryParse(branchIdClaim, out var branchId) ? branchId : 0;
    }
}
