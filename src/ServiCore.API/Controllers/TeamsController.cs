using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Teams.DTOs;
using ServiCore.Application.Teams.Interfaces;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(
        ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageTeams")]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
    {
        var result = await _teamService.CreateAsync(
            request.Name,
            request.Description,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                error = result.Error
            });
        }

        return Ok(result.Value);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result =
            await _teamService.GetAllAsync(
                cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                error = result.Error
            });
        }

        return Ok(result.Value);
    }
    [HttpGet("{teamId:guid}")]
    public async Task<IActionResult> GetById(Guid teamId, CancellationToken cancellationToken)
    {
        var result =
            await _teamService.GetByIdAsync(
                teamId,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new
            {
                error = result.Error
            });
        }

        return Ok(result.Value);
    }
    [HttpPut("{teamId:guid}")]
    [Authorize(Policy = "CanManageTeams")]
    public async Task<IActionResult> Update(Guid teamId, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        var result =
            await _teamService.UpdateAsync(
                teamId,
                request.Name,
                request.Description,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new
            {
                error = result.Error
            });
        }

        return Ok(result.Value);
    }
    [HttpDelete("{teamId:guid}")]
    [Authorize(Policy = "CanManageTeams")]
    public async Task<IActionResult> Deactivate(Guid teamId, CancellationToken cancellationToken)
    {
        var result =
            await _teamService.DeactivateAsync(
                teamId,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new
            {
                error = result.Error
            });
        }

        return NoContent();
    }
}