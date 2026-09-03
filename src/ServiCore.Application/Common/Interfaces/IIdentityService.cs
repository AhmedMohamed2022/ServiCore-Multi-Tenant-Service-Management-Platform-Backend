using ServiCore.Application.Common.Results;

namespace ServiCore.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
    Task<Result<Guid>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}