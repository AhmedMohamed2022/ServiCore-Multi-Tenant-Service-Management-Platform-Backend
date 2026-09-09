namespace ServiCore.Application.Reporting.DTOs;

public sealed record DashboardOverviewDto(
    TicketOverviewDto TicketOverview,
    TicketPriorityStatisticsDto PriorityStatistics,
    TicketPerformanceDto Performance,
    OrganizationCountsDto OrganizationCounts);