using Microsoft.AspNetCore.SignalR;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Notifications.DTOs;

namespace ServiCore.API.Hubs;

public class NotificationRealtimePublisher
    : INotificationRealtimePublisher
{
    private const string NotificationReceivedEvent =
        "notification:new";

    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationRealtimePublisher(
        IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishAsync(
        NotificationRealtimeDto notification,
        CancellationToken cancellationToken = default)
    {
        var payload = new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.RelatedEntityId,
            notification.CreatedAt,
            notification.ReadAt,
            notification.IsRead);

        var groupName =
            NotificationHub.GetUserGroupName(
                notification.OrganizationId,
                notification.UserId);

        return _hubContext
            .Clients
            .Group(groupName)
            .SendAsync(
                NotificationReceivedEvent,
                payload,
                cancellationToken);
    }
}