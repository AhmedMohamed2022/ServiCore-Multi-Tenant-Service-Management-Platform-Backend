using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;

namespace ServiCore.UnitTests;

public sealed class NotificationTests
{
    [Fact]
    public void Constructor_ShouldCreateUnreadNotification()
    {
        var notification = new Notification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationType.TicketAssigned,
            " Ticket assigned ",
            " You have a new ticket. ");

        Assert.NotEqual(Guid.Empty, notification.Id);
        Assert.Equal("Ticket assigned", notification.Title);
        Assert.Equal("You have a new ticket.", notification.Message);
        Assert.False(notification.IsRead);
        Assert.Null(notification.ReadAt);
    }

    [Fact]
    public void MarkAsRead_ShouldSetReadAt()
    {
        var notification = new Notification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationType.TicketCommentAdded,
            "Comment",
            "A new comment was added.");

        notification.MarkAsRead();

        Assert.True(notification.IsRead);
        Assert.NotNull(notification.ReadAt);
    }

    [Fact]
    public void MarkAsRead_ShouldBeIdempotent()
    {
        var notification = new Notification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationType.TicketStatusChanged,
            "Status changed",
            "The ticket status changed.");

        notification.MarkAsRead();
        var firstReadAt = notification.ReadAt;

        notification.MarkAsRead();

        Assert.Equal(firstReadAt, notification.ReadAt);
    }
}
