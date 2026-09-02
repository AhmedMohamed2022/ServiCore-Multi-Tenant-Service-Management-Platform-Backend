using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Organizations.DTOs;
using ServiCore.Application.Organizations.Interfaces;

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

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _organizationService.CreateAsync(
            request.Name,
            cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new
            {
                error = result.Error
            });

        return CreatedAtAction(
            nameof(Create),
            new { id = result.Value!.Id },
            result.Value);
    }
}