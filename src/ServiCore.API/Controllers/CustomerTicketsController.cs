using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Tickets.DTOs;
using ServiCore.Application.Tickets.Interfaces;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/customer-tickets")]
[Authorize]
public class CustomerTicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public CustomerTicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(
        CreateCustomerTicketRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.CreateForCustomerAsync(
                request,
                cancellationToken);

            return Created(
                $"/api/tickets/{ticket.Id}",
                ticket);
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
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { error = ex.Message });
        }
    }
}
