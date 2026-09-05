namespace ServiCore.Application.Common.Interfaces;

public interface IInvitationTokenService
{
    string GenerateToken();

    string HashToken(string token);
}