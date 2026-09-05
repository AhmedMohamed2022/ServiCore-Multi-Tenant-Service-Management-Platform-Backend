using ServiCore.Application.Tickets.DTOs;
using ServiCore.Domain.Enums;

namespace ServiCore.Application.Tickets.Interfaces;

public interface ITicketService
{
    Task<TicketDto> CreateAsync(
        CreateTicketRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<TicketDto> GetByIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<TicketDto> UpdateAsync(
        Guid ticketId,
        UpdateTicketRequest request,
        CancellationToken cancellationToken = default);

    Task<TicketDto> OpenAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);
    Task<TicketDto> AssignAsync(
    Guid ticketId,
    Guid agentId,
    CancellationToken cancellationToken = default);

    Task<TicketDto> UnassignAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<TicketDto> StartProgressAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<TicketDto> WaitForCustomerAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<TicketDto> ResolveAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<TicketDto> CloseAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);
}