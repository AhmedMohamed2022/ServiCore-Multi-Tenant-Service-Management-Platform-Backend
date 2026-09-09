namespace ServiCore.Application.Reporting.DTOs;

public sealed record TicketPerformanceDto(
    int ResolvedCount,
    int ClosedCount,
    TimeSpan? AverageResolutionTime,
    TimeSpan? AverageClosureTime);