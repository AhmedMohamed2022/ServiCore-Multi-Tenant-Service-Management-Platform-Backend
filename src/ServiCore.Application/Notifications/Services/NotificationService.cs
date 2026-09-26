using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Notifications.DTOs;
using ServiCore.Application.Notifications.Interfaces;
using ServiCore.Domain.Entities;

namespace ServiCore.Application.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly INotificationRealtimePublisher _realtimePublisher;

    public NotificationService(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ITenantContext tenantContext,
        INotificationRealtimePublisher realtimePublisher)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _realtimePublisher = realtimePublisher;

    }

    public async Task<NotificationDto> CreateAsync(
    CreateNotificationRequest request,
    CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var recipientIsOrganizationMember =
            await _dbContext.OrganizationMemberExistsAsync(
                organizationId,
                request.UserId,
                cancellationToken);

        var recipientIsActiveCustomer =
            await _dbContext.ActiveCustomerLinkedToOrganizationAsync(
                organizationId,
                request.UserId,
                cancellationToken);

        if (!recipientIsOrganizationMember && !recipientIsActiveCustomer)
        {
            throw new UnauthorizedAccessException(
                "The notification recipient is not a member of this organization.");
        }

        var notification = new Notification(
            request.UserId,
            organizationId,
            request.Type,
            request.Title,
            request.Message,
            request.RelatedEntityId);

        _dbContext.AddNotification(notification);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var notificationDto = Map(notification);

        var realtimeNotification =
            new NotificationRealtimeDto(
                notification.UserId,
                notification.OrganizationId,
                notification.Id,
                notification.Type,
                notification.Title,
                notification.Message,
                notification.RelatedEntityId,
                notification.CreatedAt,
                notification.ReadAt,
                notification.IsRead);

        await _realtimePublisher.PublishAsync(
            realtimeNotification,
            cancellationToken);

        return notificationDto;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var userId = GetCurrentUserId();

        var notifications =
            await _dbContext.GetNotificationsAsync(
                organizationId,
                userId,
                cancellationToken);

        return notifications
            .Select(Map)
            .ToList();
    }

    public async Task MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var userId = GetCurrentUserId();

        var notification =
            await _dbContext.GetNotificationAsync(
                organizationId,
                userId,
                notificationId,
                cancellationToken);

        if (notification is null)
        {
            throw new KeyNotFoundException(
                "Notification was not found.");
        }

        notification.MarkAsRead();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var userId = GetCurrentUserId();

        var notifications =
            await _dbContext.GetNotificationsAsync(
                organizationId,
                userId,
                cancellationToken);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "The current user is not authenticated.");
        }

        return _currentUser.UserId.Value;
    }

    private Guid GetOrganizationId()
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            throw new InvalidOperationException(
                "The current organization has not been resolved.");
        }

        return _tenantContext.OrganizationId.Value;
    }

    private static NotificationDto Map(
        Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.RelatedEntityId,
            notification.CreatedAt,
            notification.ReadAt,
            notification.IsRead);
    }
}