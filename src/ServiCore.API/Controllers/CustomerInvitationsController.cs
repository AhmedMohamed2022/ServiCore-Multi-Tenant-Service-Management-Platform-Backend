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
                message ="Customer invitation created.",
                
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
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
    [HttpGet("preview")]
    public async Task<IActionResult> Preview(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        try
        {
            var preview = await _service.PreviewAsync(
                token,
                cancellationToken);

            return Ok(preview);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new
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
            var result = await _service.AcceptAsync(
                request,
                cancellationToken);

            return Ok(new
            {
                message = result.ExistingAccount
                    ? "Invitation accepted. Sign in with your existing password."
                    : "Customer account is ready. You can now login.",
                existingAccount = result.ExistingAccount
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