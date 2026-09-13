namespace ServiCore.Application.Reporting.DTOs;

public sealed record CategoryStatisticsDto(
    Guid CategoryId,
    string CategoryName,
    int TotalTickets,
    int ActiveTickets,
    int ResolvedTickets,
    int ClosedTickets);