using ServiCore.Application.Authentication.DTOs;
using ServiCore.Application.Authentication.Interfaces;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;
using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;

namespace ServiCore.Application.Authentication.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public AuthenticationService(
        IApplicationDbContext dbContext,
        IIdentityService identityService,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<RegisterResponse>.Failure(
                "Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.OrganizationName))
        {
            return Result<RegisterResponse>.Failure(
                "Organization name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<RegisterResponse>.Failure(
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<RegisterResponse>.Failure(
                "Password is required.");
        }

        await using var transaction =
            await _dbContext.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var userResult =
                await _identityService.CreateUserAsync(
                    request.Email.Trim(),
                    request.Password,
                    cancellationToken);
            if (!userResult.IsSuccess)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                return Result<RegisterResponse>.Failure(
                    userResult.Error!);
            }

            var userId = userResult.Value;

            var organization = new Organization(
                request.OrganizationName);

            organization.SetOwner(userId);

            _dbContext.AddOrganization(organization);

            var membership = new OrganizationMember(
                organization.Id,
                userId,
                OrganizationRole.Owner);

            _dbContext.AddOrganizationMember(
                membership);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            var response = new RegisterResponse(
                userId,
                organization.Id,
                organization.Name);

            return Result<RegisterResponse>.Success(
                response);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
    public async Task<Result<string>> LoginAsync(
    string email,
    string password,
    CancellationToken cancellationToken = default)
    {
        var result =
            await _identityService.ValidateCredentialsAsync(
                email.Trim(),
                password,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return Result<string>.Failure(
                result.Error!);
        }

        var userId = result.Value;

        var tokenResult = _tokenService.GenerateToken(
            userId,
            email.Trim());

        return tokenResult;
    }
}