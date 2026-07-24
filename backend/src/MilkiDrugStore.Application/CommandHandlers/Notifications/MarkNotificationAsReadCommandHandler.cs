using MediatR;
using MilkiDrugStore.Application.Commands.Notifications;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Application.CommandHandlers.Notifications;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly INotificationService _notificationService;

    public MarkNotificationAsReadCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(request.NotificationId);
        return true;
    }
}
