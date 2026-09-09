namespace ServiCore.Application.Reporting.Queries;

public sealed record AgentStatisticsQueryResult(
    Guid AgentId,
    string AgentUserName,
    int AssignedTickets,
    int ActiveTickets,
    int ResolvedTickets,
    int ClosedTickets);