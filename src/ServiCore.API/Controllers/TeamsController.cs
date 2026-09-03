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
    public async Task<IActionResult> Create(
        [FromBody] CreateTeamRequest request,
        CancellationToken cancellationToken)
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
}