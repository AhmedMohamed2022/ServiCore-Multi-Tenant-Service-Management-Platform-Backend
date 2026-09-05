using ServiCore.Domain.Enums;

namespace ServiCore.Application.Tickets.DTOs;

public sealed record CreateTicketRequest(
    Guid CustomerId,
    Guid TeamId,
    Guid CategoryId,
    string Title,
    string Description,
    TicketPriority Priority);