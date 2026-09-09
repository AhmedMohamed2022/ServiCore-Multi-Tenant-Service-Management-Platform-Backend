namespace ServiCore.Application.Reporting.Queries;

public sealed record TeamStatisticsQueryResult(
    Guid TeamId,
    string TeamName,
    int MemberCount,
    int TotalTickets,
    int ActiveTickets,
    int ResolvedTickets,
    int ClosedTickets);