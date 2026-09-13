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
    [HttpGet("customers")]
    public async Task<ActionResult<IReadOnlyList<CustomerStatisticsDto>>>
    GetCustomerStatistics(
        [FromQuery] ReportDateRangeRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _reportingService.GetCustomerStatisticsAsync(
                request,
                cancellationToken);

        return Ok(result);
    }
    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<CategoryStatisticsDto>>>
    GetCategoryStatistics(
        [FromQuery] ReportDateRangeRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _reportingService.GetCategoryStatisticsAsync(
                request,
                cancellationToken);

        return Ok(result);
    }
    [HttpGet("tickets/time-series")]
    public async Task<ActionResult<IReadOnlyList<TicketTimeSeriesDto>>>
    GetTicketTimeSeries(
        [FromQuery] ReportDateRangeRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _reportingService.GetTicketTimeSeriesAsync(
                request,
                cancellationToken);

        return Ok(result);
    }
}