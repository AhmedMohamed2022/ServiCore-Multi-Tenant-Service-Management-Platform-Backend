namespace ServiCore.Application.Reporting.DTOs;

public sealed record AgentStatisticsDto(
    Guid AgentId,
    string AgentUserName,
    int AssignedTickets,
    int ActiveTickets,
    int ResolvedTickets,
    int ClosedTickets);