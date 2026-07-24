using MediatR;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.DTOs.Notification;

namespace MilkiDrugStore.Application.Queries.Notifications;

public record GetNotificationsQuery(bool? UnreadOnly = null) : IRequest<IEnumerable<NotificationResponse>>;
