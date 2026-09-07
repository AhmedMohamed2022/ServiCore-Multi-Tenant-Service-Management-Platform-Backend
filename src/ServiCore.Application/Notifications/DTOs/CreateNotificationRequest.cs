using ServiCore.Domain.Enums;

namespace ServiCore.Application.Notifications.DTOs;

public sealed record CreateNotificationRequest(
    Guid UserId,
    NotificationType Type,
    string Title,
    string Message,
    Guid? RelatedEntityId = null);