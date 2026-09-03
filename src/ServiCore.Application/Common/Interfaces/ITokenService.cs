using ServiCore.Application.Common.Results;

namespace ServiCore.Application.Common.Interfaces;

public interface ITokenService
{
    Result<string> GenerateToken(Guid userId, string email);
}