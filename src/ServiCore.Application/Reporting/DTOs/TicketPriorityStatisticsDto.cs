namespace ServiCore.Application.Reporting.DTOs;

public sealed record TicketPriorityStatisticsDto(
    int Low,
    int Medium,
    int High,
    int Critical);