using ServiCore.Domain.Common;
using ServiCore.Domain.Enums;

namespace ServiCore.Domain.Entities;

public class Notification : Entity
{
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public Guid? RelatedEntityId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public Notification(
        Guid userId,
        Guid organizationId,
        NotificationType type,
        string title,
        string message,
        Guid? relatedEntityId = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));

        if (organizationId == Guid.Empty)
            throw new ArgumentException(
                "Organization ID is required.",
                nameof(organizationId));

        if (!Enum.IsDefined(type))
            throw new ArgumentException(
                "A valid notification type is required.",
                nameof(type));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Notification title is required.",
                nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException(
                "Notification message is required.",
                nameof(message));

        Id = Guid.NewGuid();
        UserId = userId;
        OrganizationId = organizationId;
        Type = type;
        Title = title.Trim();
        Message = message.Trim();
        RelatedEntityId = relatedEntityId;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsRead => ReadAt.HasValue;

    public void MarkAsRead()
    {
        if (ReadAt.HasValue)
            return;

        ReadAt = DateTime.UtcNow;
    }
}