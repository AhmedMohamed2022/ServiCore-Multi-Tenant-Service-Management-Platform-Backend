using ServiCore.Application.Notifications.DTOs;

namespace ServiCore.Application.Notifications.Interfaces;

public interface INotificationService
{
    Task<NotificationDto> CreateAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(
        CancellationToken cancellationToken = default);
}