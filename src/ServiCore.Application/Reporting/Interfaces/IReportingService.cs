using ServiCore.Application.Reporting.DTOs;

namespace ServiCore.Application.Reporting.Interfaces;

public interface IReportingService
{
    Task<DashboardOverviewDto> GetDashboardAsync(
        ReportDateRangeRequest request,
        CancellationToken cancellationToken = default);

    Task<TicketStatisticsDto> GetTicketStatisticsAsync(
        ReportDateRangeRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeamStatisticsDto>> GetTeamStatisticsAsync(
        ReportDateRangeRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgentStatisticsDto>> GetAgentStatisticsAsync(
        ReportDateRangeRequest request,
        CancellationToken cancellationToken = default);
}