using ServiCore.Domain.Enums;

namespace ServiCore.Application.Tickets.DTOs;

public sealed record CreateCustomerTicketRequest(
    Guid CategoryId,
    string Title,
    string Description,
    TicketPriority Priority);
