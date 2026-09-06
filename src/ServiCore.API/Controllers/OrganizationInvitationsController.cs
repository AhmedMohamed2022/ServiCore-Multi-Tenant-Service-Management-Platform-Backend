using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.OrganizationInvitations.DTOs;
using ServiCore.Application.OrganizationInvitations.Interfaces;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/organization/invitations")]
[Authorize]
public class OrganizationInvitationsController : ControllerBase
{
    private readonly IOrganizationInvitationService _service;

    public OrganizationInvitationsController(
        IOrganizationInvitationService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageStaff")]
    public async Task<IActionResult> Invite(
        InviteOrganizationMemberRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.InviteAsync(
                request,
                cancellationToken);

            return Ok(new
            {
                invitation = result.Invitation,

                // Development/testing only.
                invitationToken = result.Token
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                error = ex.Message
            });
        }
    }

    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<IActionResult> Accept(
        AcceptOrganizationInvitationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.AcceptAsync(
                request,
                cancellationToken);

            return Ok(new
            {
                message =
                    "Invitation accepted successfully."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                error = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                error = ex.Message
            });
        }
    }

    [HttpGet]
    [Authorize(Policy = "CanManageStaff")]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var invitations = await _service.GetAllAsync(
            cancellationToken);

        return Ok(invitations);
    }

    [HttpPost("{invitationId:guid}/revoke")]
    [Authorize(Policy = "CanManageStaff")]
    public async Task<IActionResult> Revoke(
        Guid invitationId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.RevokeAsync(
                invitationId,
                cancellationToken);

            return Ok(new
            {
                message =
                    "Invitation revoked successfully."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                error = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                error = ex.Message
            });
        }
    }
}