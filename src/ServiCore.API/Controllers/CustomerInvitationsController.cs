using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.CustomerInvitations.DTOs;
using ServiCore.Application.CustomerInvitations.Interfaces;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/customer-invitations")]
[Authorize]
public class CustomerInvitationsController : ControllerBase
{
    private readonly ICustomerInvitationService _service;

    public CustomerInvitationsController(
        ICustomerInvitationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Invite(
        [FromBody] InviteCustomerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token =
                await _service.InviteAsync(
                    request,
                    cancellationToken);

            return Ok(new
            {
                message =
                    "Customer invitation created.",
                token
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
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
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpPost("accept")]
    public async Task<IActionResult> Accept(
        [FromBody] AcceptCustomerInvitationRequest request,
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
                    "Customer account is ready. You can now login."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpPost("{invitationId:guid}/revoke")]
    public async Task<IActionResult> Revoke(
        Guid invitationId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.RevokeAsync(
                invitationId,
                cancellationToken);

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
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
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }
}