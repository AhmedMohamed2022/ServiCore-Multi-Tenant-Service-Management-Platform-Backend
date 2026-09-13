namespace ServiCore.Application.Reporting.Queries;

public sealed record CategoryStatisticsQueryResult(
    Guid CategoryId,
    string CategoryName,
    int TotalTickets,
    int ActiveTickets,
    int ResolvedTickets,
    int ClosedTickets);