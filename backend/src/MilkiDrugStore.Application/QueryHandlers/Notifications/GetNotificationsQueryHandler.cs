using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Queries.Notifications;
using MilkiDrugStore.Application.DTOs.Notification;

namespace MilkiDrugStore.Application.QueryHandlers.Notifications;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, IEnumerable<NotificationResponse>>
{
    private readonly INotificationService _notificationService;

    public GetNotificationsQueryHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<NotificationResponse>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationService.GetAllAsync();
        if (request.UnreadOnly == true)
        {
            notifications = notifications.Where(n => !n.IsRead);
        }
        return notifications;
    }
}
