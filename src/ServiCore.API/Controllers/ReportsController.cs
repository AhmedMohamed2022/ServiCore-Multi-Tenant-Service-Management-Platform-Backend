using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Reporting.DTOs;
using ServiCore.Application.Reporting.Interfaces;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = "CanViewReports")]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportsController(
        IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboard(
        [FromQuery] ReportDateRangeRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _reportingService.GetDashboardAsync(
                request,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("tickets")]
    public async Task<ActionResult<TicketStatisticsDto>> GetTicketStatistics(
        [FromQuery] ReportDateRangeRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _reportingService.GetTicketStatisticsAsync(
                request,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("teams")]
    public async Task<ActionResult<IReadOnlyList<TeamStatisticsDto>>> GetTeamStatistics(
        [FromQuery] ReportDateRangeRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _reportingService.GetTeamStatisticsAsync(
                request,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("agents")]
    public async Task<ActionResult<IReadOnlyList<AgentStatisticsDto>>> GetAgentStatistics(
        [FromQuery] ReportDateRangeRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _reportingService.GetAgentStatisticsAsync(
                request,
                cancellationToken);

        return Ok(result);
    }
}