namespace ServiCore.Application.Reporting.Queries;

public sealed record CustomerStatisticsQueryResult(
    Guid CustomerId,
    string CustomerName,
    int TotalTickets,
    int ActiveTickets,
    int ResolvedTickets,
    int ClosedTickets);