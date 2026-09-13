using ServiCore.Application.Tickets.DTOs;

namespace ServiCore.Application.Common.Interfaces;

public interface ITicketCommentRealtimePublisher
{
    Task PublishAsync(
        Guid userId,
        Guid organizationId,
        TicketCommentDto comment,
        CancellationToken cancellationToken = default);
}