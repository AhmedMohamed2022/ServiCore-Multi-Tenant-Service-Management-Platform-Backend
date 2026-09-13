namespace ServiCore.Application.Reporting.DTOs;

public sealed record CustomerStatisticsDto(
    Guid CustomerId,
    string CustomerName,
    int TotalTickets,
    int ActiveTickets,
    int ResolvedTickets,
    int ClosedTickets);