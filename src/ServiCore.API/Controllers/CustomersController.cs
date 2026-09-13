using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Customers.DTOs;
using ServiCore.Application.Customers.Interfaces;
using System.Security.Claims;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(
        ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // Not tenant-scoped on purpose — this is how a newly-logged-in customer
    // (who has no X-Organization-Id yet) discovers which organization(s)
    // they belong to. Registered as exempt in TenantResolutionMiddleware.
    // Must stay above the "{customerId:guid}" route only for readability;
    // the :guid constraint already means "mine" can never match it.
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result =
            await _customerService.GetMineAsync(
                userId,
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

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _customerService.CreateAsync(
                request,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                error = result.Error
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { customerId = result.Value!.Id },
            result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result =
            await _customerService.GetAllAsync(
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

    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> GetById(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var result =
            await _customerService.GetByIdAsync(
                customerId,
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

    [HttpPut("{customerId:guid}")]
    public async Task<IActionResult> Update(
        Guid customerId,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _customerService.UpdateAsync(
                customerId,
                request,
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

    [HttpDelete("{customerId:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var result =
            await _customerService.DeactivateAsync(
                customerId,
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