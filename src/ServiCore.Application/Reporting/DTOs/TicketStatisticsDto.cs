namespace ServiCore.Application.Reporting.DTOs;

public sealed record TicketStatisticsDto(
    IReadOnlyList<TicketStatusStatisticDto> StatusDistribution,
    IReadOnlyList<TicketPriorityStatisticDto> PriorityDistribution,
    IReadOnlyList<CategoryTicketStatisticDto> CategoryDistribution,
    TicketPerformanceDto Performance);