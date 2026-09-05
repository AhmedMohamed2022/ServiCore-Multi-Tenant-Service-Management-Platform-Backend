using ServiCore.Domain.Enums;

namespace ServiCore.Application.Tickets.DTOs;

public sealed record TicketDto(
    Guid Id,
    Guid OrganizationId,
    Guid CustomerId,
    Guid TeamId,
    Guid? AssignedAgentId,
    Guid CategoryId,
    string Title,
    string Description,
    TicketPriority Priority,
    TicketStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt);