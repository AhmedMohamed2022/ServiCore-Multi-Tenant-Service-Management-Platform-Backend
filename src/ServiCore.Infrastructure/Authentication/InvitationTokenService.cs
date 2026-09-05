using System.Security.Cryptography;
using System.Text;
using ServiCore.Application.Common.Interfaces;

namespace ServiCore.Infrastructure.Authentication;

public class InvitationTokenService : IInvitationTokenService
{
    public string GenerateToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}