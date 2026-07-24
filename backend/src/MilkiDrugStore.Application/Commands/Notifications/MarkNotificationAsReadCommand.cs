using MediatR;
using MilkiDrugStore.Application.DTOs.Notification;

namespace MilkiDrugStore.Application.Commands.Notifications;

public record MarkNotificationAsReadCommand(int NotificationId) : IRequest<bool>;
