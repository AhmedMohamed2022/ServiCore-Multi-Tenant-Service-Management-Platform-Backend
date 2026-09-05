using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Tickets.DTOs;
using ServiCore.Application.Tickets.Interfaces;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(
        CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { ticketId = ticket.Id },
                ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var tickets = await _ticketService.GetAllAsync(
            cancellationToken);

        return Ok(tickets);
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<ActionResult<TicketDto>> GetById(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.GetByIdAsync(
                ticketId,
                cancellationToken);

            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{ticketId:guid}")]
    [Authorize(Policy = "CanManageTickets")]
    public async Task<ActionResult<TicketDto>> Update(
        Guid ticketId,
        UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.UpdateAsync(
                ticketId,
                request,
                cancellationToken);

            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{ticketId:guid}/open")]
    [Authorize(Policy = "CanManageTickets")]
    public async Task<ActionResult<TicketDto>> Open(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.OpenAsync(
                ticketId,
                cancellationToken);

            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }
    [HttpPost("{ticketId:guid}/assign")]
    [Authorize(Policy = "CanManageTickets")]
    public async Task<ActionResult<TicketDto>> Assign(
        Guid ticketId,
        AssignTicketRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.AssignAsync(
                ticketId,
                request.AgentId,
                cancellationToken);

            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{ticketId:guid}/unassign")]
    [Authorize(Policy = "CanManageTickets")]
    public async Task<ActionResult<TicketDto>> Unassign(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.UnassignAsync(
                ticketId,
                cancellationToken);

            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{ticketId:guid}/start")]
    [Authorize(Policy = "CanWorkAssignedTickets")]
    public async Task<ActionResult<TicketDto>> StartProgress(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.StartProgressAsync(
                ticketId,
                cancellationToken);

            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
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
                message = ex.Message
            });
        }
    }

    [HttpPost("{ticketId:guid}/wait-for-customer")]
    [Authorize(Policy = "CanWorkAssignedTickets")]
    public async Task<ActionResult<TicketDto>> WaitForCustomer(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.WaitForCustomerAsync(
                ticketId,
                cancellationToken);

            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
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
                message = ex.Message
            });
        }
    }

    [HttpPost("{ticketId:guid}/resolve")]
    [Authorize(Policy = "CanWorkAssignedTickets")]
    public async Task<ActionResult<TicketDto>> Resolve(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.ResolveAsync(
                ticketId,
                cancellationToken);

            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
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
                message = ex.Message
            });
        }
    }
    [HttpPost("{ticketId:guid}/close")]
    [Authorize(Policy = "CanManageTickets")]
    public async Task<ActionResult<TicketDto>> Close(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.CloseAsync(
                ticketId,
                cancellationToken);

            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }
}