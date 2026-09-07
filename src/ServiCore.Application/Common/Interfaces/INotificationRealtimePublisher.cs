using ServiCore.Application.Notifications.DTOs;

namespace ServiCore.Application.Common.Interfaces;

public interface INotificationRealtimePublisher
{
    Task PublishAsync(
        NotificationRealtimeDto notification,
        CancellationToken cancellationToken = default);
}