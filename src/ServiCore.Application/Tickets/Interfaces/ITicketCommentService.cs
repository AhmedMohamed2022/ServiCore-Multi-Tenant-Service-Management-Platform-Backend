using ServiCore.Application.Tickets.DTOs;

namespace ServiCore.Application.Tickets.Interfaces;

public interface ITicketCommentService
{
    Task<TicketCommentDto> AddAsync(
        Guid ticketId,
        AddTicketCommentRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketCommentDto>> GetAllAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);
}