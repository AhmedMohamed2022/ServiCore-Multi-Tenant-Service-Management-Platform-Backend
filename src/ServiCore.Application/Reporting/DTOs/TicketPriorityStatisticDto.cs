using ServiCore.Domain.Enums;

namespace ServiCore.Application.Reporting.DTOs;

public sealed record TicketPriorityStatisticDto(
    TicketPriority Priority,
    int Count);