using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiCore.Application.Authentication.DTOs;
using ServiCore.Application.Authentication.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ServiCore.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(
        IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RegisterAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                error = result.Error
            });
        }

        return Ok(result.Value);
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(
    [FromBody] LoginRequest request,
    CancellationToken cancellationToken)
    {
        var result =
            await _authenticationService.LoginAsync(
                request.Email,
                request.Password,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new
            {
                error = result.Error
            });
        }

        return Ok(new
        {
            token = result.Value
        });
    }
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier),

            email = User.FindFirstValue(
                JwtRegisteredClaimNames.Email)
                ?? User.FindFirstValue(ClaimTypes.Email)
        });
    }
}