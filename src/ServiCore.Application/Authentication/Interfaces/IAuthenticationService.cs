using ServiCore.Application.Authentication.DTOs;
using ServiCore.Application.Common.Results;

namespace ServiCore.Application.Authentication.Interfaces;

public interface IAuthenticationService
{
    Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);
    Task<Result<string>> LoginAsync(
      string email,
      string password,
      CancellationToken cancellationToken = default);
}