using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Reporting.DTOs;
using ServiCore.Application.Reporting.Interfaces;

namespace ServiCore.Application.Reporting.Services;

public sealed class ReportingService : IReportingService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ReportingService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<DashboardOverviewDto> GetDashboardAsync(
        ReportDateRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(request);

        var organizationId = GetOrganizationId();

        var ticketStatistics =
            await _dbContext.GetDashboardTicketStatisticsAsync(
                organizationId,
                request.From,
                request.To,
                cancellationToken);

        var organizationCounts =
            await _dbContext.GetOrganizationReportingCountsAsync(
                organizationId,
                cancellationToken);

        var ticketOverview = new TicketOverviewDto(
            TotalTickets: ticketStatistics.TotalTickets,
            NewTickets: ticketStatistics.NewTickets,
            OpenTickets: ticketStatistics.OpenTickets,
            InProgressTickets: ticketStatistics.InProgressTickets,
            WaitingForCustomerTickets:
                ticketStatistics.WaitingForCustomerTickets,
            ResolvedTickets: ticketStatistics.ResolvedTickets,
            ClosedTickets: ticketStatistics.ClosedTickets);

        var priorityStatistics = new TicketPriorityStatisticsDto(
            Low: ticketStatistics.LowPriorityTickets,
            Medium: ticketStatistics.MediumPriorityTickets,
            High: ticketStatistics.HighPriorityTickets,
            Critical: ticketStatistics.CriticalPriorityTickets);

        var performance = new TicketPerformanceDto(
            ResolvedCount: ticketStatistics.ResolvedCount,
            ClosedCount: ticketStatistics.ClosedCount,
            AverageResolutionTime:
                ToTimeSpan(
                    ticketStatistics.AverageResolutionSeconds),
            AverageClosureTime:
                ToTimeSpan(
                    ticketStatistics.AverageClosureSeconds));

        var organizationCountsDto = new OrganizationCountsDto(
            TotalCustomers: organizationCounts.TotalCustomers,
            TotalCategories: organizationCounts.TotalCategories,
            TotalTeams: organizationCounts.TotalTeams,
            TotalAgents: organizationCounts.TotalAgents);

        return new DashboardOverviewDto(
            TicketOverview: ticketOverview,
            PriorityStatistics: priorityStatistics,
            Performance: performance,
            OrganizationCounts: organizationCountsDto);
    }

    public async Task<TicketStatisticsDto> GetTicketStatisticsAsync(
     ReportDateRangeRequest request,
     CancellationToken cancellationToken = default)
    {
        ValidateDateRange(request);

        var organizationId = GetOrganizationId();

        var result =
            await _dbContext.GetTicketStatisticsAsync(
                organizationId,
                request.From,
                request.To,
                cancellationToken);

        var statusDistribution =
            result.StatusDistribution
                .Select(statistic =>
                    new TicketStatusStatisticDto(
                        statistic.Status,
                        statistic.Count))
                .ToList();

        var priorityDistribution =
            result.PriorityDistribution
                .Select(statistic =>
                    new TicketPriorityStatisticDto(
                        statistic.Priority,
                        statistic.Count))
                .ToList();

        var categoryDistribution =
            result.CategoryDistribution
                .Select(statistic =>
                    new CategoryTicketStatisticDto(
                        statistic.CategoryId,
                        statistic.CategoryName,
                        statistic.TicketCount))
                .ToList();

        var performance = new TicketPerformanceDto(
            ResolvedCount: result.ResolvedCount,
            ClosedCount: result.ClosedCount,
            AverageResolutionTime:
                ToTimeSpan(result.AverageResolutionSeconds),
            AverageClosureTime:
                ToTimeSpan(result.AverageClosureSeconds));

        return new TicketStatisticsDto(
            StatusDistribution: statusDistribution,
            PriorityDistribution: priorityDistribution,
            CategoryDistribution: categoryDistribution,
            Performance: performance);
    }

    public async Task<IReadOnlyList<TeamStatisticsDto>>
    GetTeamStatisticsAsync(
        ReportDateRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(request);

        var organizationId = GetOrganizationId();

        var result =
            await _dbContext.GetTeamStatisticsAsync(
                organizationId,
                request.From,
                request.To,
                cancellationToken);

        return result
            .Select(team =>
                new TeamStatisticsDto(
                    team.TeamId,
                    team.TeamName,
                    team.MemberCount,
                    team.TotalTickets,
                    team.ActiveTickets,
                    team.ResolvedTickets,
                    team.ClosedTickets))
            .ToList();
    }

    public async Task<IReadOnlyList<AgentStatisticsDto>>
    GetAgentStatisticsAsync(
        ReportDateRangeRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(request);

        var organizationId = GetOrganizationId();

        var result =
            await _dbContext.GetAgentStatisticsAsync(
                organizationId,
                request.From,
                request.To,
                cancellationToken);

        return result
            .Select(agent =>
                new AgentStatisticsDto(
                    agent.AgentId,
                    agent.AgentUserName,
                    agent.AssignedTickets,
                    agent.ActiveTickets,
                    agent.ResolvedTickets,
                    agent.ClosedTickets))
            .ToList();
    }

    private Guid GetOrganizationId()
    {
        return _tenantContext.OrganizationId
            ?? throw new InvalidOperationException(
                "No organization context is available.");
    }

    private static void ValidateDateRange(
        ReportDateRangeRequest request)
    {
        if (request.From.HasValue &&
            request.To.HasValue &&
            request.From.Value > request.To.Value)
        {
            throw new ArgumentException(
                "'From' date cannot be later than 'To' date.");
        }
    }

    private static TimeSpan? ToTimeSpan(
        double? totalSeconds)
    {
        return totalSeconds.HasValue
            ? TimeSpan.FromSeconds(totalSeconds.Value)
            : null;
    }
}