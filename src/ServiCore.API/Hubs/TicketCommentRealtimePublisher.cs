using Microsoft.AspNetCore.SignalR;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Tickets.DTOs;

namespace ServiCore.API.Hubs;

public class TicketCommentRealtimePublisher
    : ITicketCommentRealtimePublisher
{
    private const string TicketCommentAddedEvent =
        "ticket:comment-added";

    private readonly IHubContext<NotificationHub> _hubContext;

    public TicketCommentRealtimePublisher(
        IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishAsync(
        Guid userId,
        Guid organizationId,
        TicketCommentDto comment,
        CancellationToken cancellationToken = default)
    {
        var groupName =
            NotificationHub.GetUserGroupName(
                organizationId,
                userId);

        return _hubContext
            .Clients
            .Group(groupName)
            .SendAsync(
                TicketCommentAddedEvent,
                comment,
                cancellationToken);
    }
}