using ServiCore.Domain.Enums;

namespace ServiCore.Application.Tickets.DTOs;

public sealed record UpdateTicketRequest(
    Guid CategoryId,
    string Title,
    string Description,
    TicketPriority Priority);