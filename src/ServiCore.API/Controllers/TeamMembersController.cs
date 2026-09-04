using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Teams.Interfaces;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/teams/{teamId:guid}/members")]
[Authorize]
public class TeamMembersController : ControllerBase
{
    private readonly ITeamMembershipService _membershipService;

    public TeamMembersController(
        ITeamMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageTeams")]
    public async Task<IActionResult> Add(
        Guid teamId,
        [FromBody] AddTeamMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _membershipService.AddMemberAsync(
                teamId,
                request.UserId,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                error = result.Error
            });
        }

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(
        Guid teamId,
        CancellationToken cancellationToken)
    {
        var result =
            await _membershipService.GetMembersAsync(
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

    [HttpDelete("{userId:guid}")]
    [Authorize(Policy = "CanManageTeams")]
    public async Task<IActionResult> Remove(
        Guid teamId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result =
            await _membershipService.RemoveMemberAsync(
                teamId,
                userId,
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

