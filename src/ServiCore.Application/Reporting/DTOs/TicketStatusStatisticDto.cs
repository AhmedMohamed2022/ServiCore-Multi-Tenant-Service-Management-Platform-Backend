using ServiCore.Domain.Enums;

namespace ServiCore.Application.Reporting.DTOs;

public sealed record TicketStatusStatisticDto(
    TicketStatus Status,
    int Count);