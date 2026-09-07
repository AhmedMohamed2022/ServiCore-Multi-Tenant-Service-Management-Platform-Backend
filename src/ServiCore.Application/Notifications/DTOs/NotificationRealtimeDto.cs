using ServiCore.Domain.Enums;

namespace ServiCore.Application.Notifications.DTOs;

public sealed record NotificationRealtimeDto(
    Guid UserId,
    Guid OrganizationId,
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    Guid? RelatedEntityId,
    DateTime CreatedAt,
    DateTime? ReadAt,
    bool IsRead);