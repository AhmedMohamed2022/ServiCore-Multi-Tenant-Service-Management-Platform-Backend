using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Tickets.DTOs;
using ServiCore.Application.Tickets.Interfaces;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:guid}/comments")]
[Authorize]
public class TicketCommentsController : ControllerBase
{
    private readonly ITicketCommentService _commentService;

    public TicketCommentsController(
        ITicketCommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketCommentDto>>> GetAll(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        try
        {
            var comments = await _commentService.GetAllAsync(
                ticketId,
                cancellationToken);

            return Ok(comments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<TicketCommentDto>> Add(
        Guid ticketId,
        AddTicketCommentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var comment = await _commentService.AddAsync(
                ticketId,
                request,
                cancellationToken);

            return Ok(comment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}