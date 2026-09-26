using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Organizations.Interfaces;
using System.Security.Claims;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(
        IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _organizationService.GetMineAsync(userId, cancellationToken);
        return Ok(result.Value);
    }

    // Backs the Owner's "add a Manager to a team" picker. Same policy as
    // staff invitations (CanManageStaff = Owner or Manager) — a Manager can
    // call this too, but only to see Agents/Managers exist; TeamMembersController
    // still rejects a Manager who tries to add anyone but an Agent.
    [HttpGet("members")]
    [Authorize(Policy = "CanManageStaff")]
    public async Task<IActionResult> GetMembers(CancellationToken cancellationToken)
    {
        var result = await _organizationService.GetMembersAsync(cancellationToken);

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