using Microsoft.Extensions.Options;
using ServiCore.Application.Common.Configuration;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.CustomerInvitations.DTOs;
using ServiCore.Application.CustomerInvitations.Interfaces;
using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;

namespace ServiCore.Application.CustomerInvitations.Services;

public class CustomerInvitationService
    : ICustomerInvitationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IInvitationTokenService _tokenService;
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;
    private readonly FrontendOptions _frontendOptions;
    public CustomerInvitationService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IInvitationTokenService tokenService,
        IIdentityService identityService,
        IEmailSender emailSender,
        IOptions<FrontendOptions> frontendOptions)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _tokenService = tokenService;
        _identityService = identityService;
        _emailSender = emailSender;
        _frontendOptions = frontendOptions.Value;
    }

    public async Task<string> InviteAsync(
        InviteCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var organizationId =
            _tenantContext.OrganizationId
            ?? throw new InvalidOperationException(
                "Organization context is required.");

        var userId =
            _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var inviterRole =
            await _dbContext.GetOrganizationRoleAsync(
                organizationId,
                userId,
                cancellationToken);

        if (inviterRole != OrganizationRole.Owner &&
            inviterRole != OrganizationRole.Manager)
        {
            throw new UnauthorizedAccessException(
                "Only owners and managers can invite customers.");
        }

        var customer =
            await _dbContext.GetCustomerForOrganizationAsync(
                organizationId,
                request.CustomerId,
                cancellationToken);

        if (customer is null)
            throw new KeyNotFoundException(
                "Customer was not found.");

        if (!customer.IsActive)
            throw new InvalidOperationException(
                "An inactive customer cannot be invited.");

        if (customer.IsLinked)
            throw new InvalidOperationException(
                "Customer already has an account.");

        var pendingInvitationExists =
            await _dbContext.PendingCustomerInvitationExistsAsync(
                organizationId,
                customer.Id,
                cancellationToken);

        if (pendingInvitationExists)
            throw new InvalidOperationException(
                "A pending invitation already exists for this customer.");

        var token =
            _tokenService.GenerateToken();

        var tokenHash =
            _tokenService.HashToken(token);

        var invitation =
            new CustomerInvitation(
                customer.Id,
                organizationId,
                customer.Email,
                tokenHash,
                DateTime.UtcNow.AddDays(2));

        _dbContext.AddCustomerInvitation(invitation);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var invitationUrl =
    $"{_frontendOptions.BaseUrl.TrimEnd('/')}" +
    $"/accept-customer-invitation?token=" +
    Uri.EscapeDataString(token);

        var emailBody = $"""
                <h2>Welcome to ServiCore</h2>

                <p>
                    You have been invited to access your customer account
                    on ServiCore.
                </p>

                <p>
                    Click the button below to accept your invitation:
                </p>

                <p>
                    <a href="{invitationUrl}"
                       style="
                           display:inline-block;
                           padding:12px 20px;
                           background:#2563eb;
                           color:white;
                           text-decoration:none;
                           border-radius:6px;
                       ">
                        Accept Customer Invitation
                    </a>
                </p>

                <p>
                    This invitation expires on
                    <strong>{invitation.ExpiresAt:u}</strong>.
                </p>

                <p>
                    If you did not expect this invitation, you can safely ignore this email.
                </p>
                """;

        await _emailSender.SendAsync(
            invitation.Email,
            "Your ServiCore customer invitation",
            emailBody,
            cancellationToken);

        return token;
    }

    public async Task AcceptAsync(
        AcceptCustomerInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException(
                "Invitation token is required.",
                nameof(request.Token));

        var tokenHash =
            _tokenService.HashToken(request.Token);

        var invitation =
            await _dbContext
                .GetCustomerInvitationByTokenHashAsync(
                    tokenHash,
                    cancellationToken);

        if (invitation is null)
            throw new KeyNotFoundException(
                "Invalid invitation token.");

        if (!invitation.IsUsable)
            throw new InvalidOperationException(
                "This invitation is no longer usable.");

        await using var transaction =
            await _dbContext.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var customer =
                await _dbContext.GetCustomerForOrganizationAsync(
                    invitation.OrganizationId,
                    invitation.CustomerId,
                    cancellationToken);

            if (customer is null)
                throw new InvalidOperationException(
                    "The customer associated with this invitation no longer exists.");

            if (customer.IsLinked)
                throw new InvalidOperationException(
                    "Customer already has an account.");

            var existingUserResult =
                await _identityService.GetUserIdByEmailAsync(
                    invitation.Email,
                    cancellationToken);

            Guid userId;

            if (existingUserResult.IsSuccess)
            {
                userId = existingUserResult.Value;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Password))
                    throw new InvalidOperationException(
                        "A password is required when creating a new account.");

                var createUserResult =
                    await _identityService.CreateUserAsync(
                        invitation.Email,
                        request.Password,
                        cancellationToken);

                if (!createUserResult.IsSuccess)
                    throw new InvalidOperationException(
                        createUserResult.Error);

                userId = createUserResult.Value;
            }

            customer.LinkUser(userId);

            invitation.Accept();

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    public async Task RevokeAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        var organizationId =
            _tenantContext.OrganizationId
            ?? throw new InvalidOperationException(
                "Organization context is required.");

        var userId =
            _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var role =
            await _dbContext.GetOrganizationRoleAsync(
                organizationId,
                userId,
                cancellationToken);

        if (role != OrganizationRole.Owner &&
            role != OrganizationRole.Manager)
        {
            throw new UnauthorizedAccessException(
                "Only owners and managers can revoke customer invitations.");
        }

        var invitation =
            await _dbContext.GetCustomerInvitationAsync(
                organizationId,
                invitationId,
                cancellationToken);

        if (invitation is null)
            throw new KeyNotFoundException(
                "Invitation was not found.");

        invitation.Revoke();

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}