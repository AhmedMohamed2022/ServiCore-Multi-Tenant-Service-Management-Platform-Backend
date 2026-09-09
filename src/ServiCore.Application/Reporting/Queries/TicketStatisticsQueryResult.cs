using ServiCore.Domain.Enums;

namespace ServiCore.Application.Reporting.Queries;

public sealed record TicketStatisticsQueryResult(
    IReadOnlyList<TicketStatusStatisticResult> StatusDistribution,
    IReadOnlyList<TicketPriorityStatisticResult> PriorityDistribution,
    IReadOnlyList<CategoryTicketStatisticResult> CategoryDistribution,
    int ResolvedCount,
    int ClosedCount,
    double? AverageResolutionSeconds,
    double? AverageClosureSeconds);

public sealed record TicketStatusStatisticResult(
    TicketStatus Status,
    int Count);

public sealed record TicketPriorityStatisticResult(
    TicketPriority Priority,
    int Count);

public sealed record CategoryTicketStatisticResult(
    Guid CategoryId,
    string CategoryName,
    int TicketCount);