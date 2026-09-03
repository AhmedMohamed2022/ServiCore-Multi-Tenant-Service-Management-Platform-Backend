using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;

namespace ServiCore.Infrastructure.Authentication;

public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(
        IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public Result<string> GenerateToken(
        Guid userId,
        string email)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            return Result<string>.Failure(
                "JWT secret key is not configured.");
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()),

            new Claim(
                JwtRegisteredClaimNames.Email,
                email),

            new Claim(
                ClaimTypes.NameIdentifier,
                userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        var tokenValue =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return Result<string>.Success(tokenValue);
    }
}