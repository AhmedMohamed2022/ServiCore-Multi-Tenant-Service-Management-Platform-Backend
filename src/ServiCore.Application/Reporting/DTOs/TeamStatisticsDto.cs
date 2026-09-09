namespace ServiCore.Application.Reporting.DTOs;

public sealed record TeamStatisticsDto(
    Guid TeamId,
    string TeamName,
    int MemberCount,
    int TotalTickets,
    int ActiveTickets,
    int ResolvedTickets,
    int ClosedTickets);