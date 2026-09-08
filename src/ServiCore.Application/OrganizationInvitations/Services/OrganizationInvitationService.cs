using ServiCore.Application.Common.Configuration;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.OrganizationInvitations.DTOs;
using ServiCore.Application.OrganizationInvitations.Interfaces;
using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;
using Microsoft.Extensions.Options;


namespace ServiCore.Application.OrganizationInvitations.Services;

public class OrganizationInvitationService : IOrganizationInvitationService
{
    private static readonly TimeSpan InvitationLifetime =
        TimeSpan.FromDays(7);

    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IInvitationTokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly FrontendOptions _frontendOptions;
    public OrganizationInvitationService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IIdentityService identityService,
        IInvitationTokenService tokenService,
        IEmailSender emailSender,
        IOptions<FrontendOptions> frontendOptions
        )
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _identityService = identityService;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _frontendOptions = frontendOptions.Value;
    }

    public async Task<(OrganizationInvitationDto Invitation, string Token)>
        InviteAsync(
            InviteOrganizationMemberRequest request,
            CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var inviterId = GetCurrentUserId();

        if (request.Role != OrganizationRole.Manager &&
            request.Role != OrganizationRole.Agent)
        {
            throw new InvalidOperationException(
                "Only Manager or Agent invitations are allowed.");
        }

        var inviterRole =
            await GetOrganizationRoleAsync(
                organizationId,
                inviterId,
                cancellationToken);

        if (inviterRole is null)
        {
            throw new UnauthorizedAccessException(
                "You are not a member of this organization.");
        }

        if (inviterRole == OrganizationRole.Agent)
        {
            throw new UnauthorizedAccessException(
                "Agents cannot invite organization members.");
        }

        if (inviterRole == OrganizationRole.Manager &&
            request.Role == OrganizationRole.Manager)
        {
            throw new UnauthorizedAccessException(
                "Managers can invite Agents only.");
        }

        var email = request.Email.Trim().ToLowerInvariant();

        var existingUserResult =
            await _identityService.GetUserIdByEmailAsync(
                email,
                cancellationToken);

        if (existingUserResult.IsSuccess)
        {
            var alreadyMember =
                await _dbContext.OrganizationMemberExistsAsync(
                    organizationId,
                    existingUserResult.Value,
                    cancellationToken);

            if (alreadyMember)
            {
                throw new InvalidOperationException(
                    "This user is already a member of the organization.");
            }
        }

        var pendingInvitationExists =
            await _dbContext.PendingInvitationExistsAsync(
                organizationId,
                email,
                cancellationToken);

        if (pendingInvitationExists)
        {
            throw new InvalidOperationException(
                "A pending invitation already exists for this email.");
        }

        var token = _tokenService.GenerateToken();

        var tokenHash =
            _tokenService.HashToken(token);

        var invitation = new OrganizationInvitation(
            organizationId,
            email,
            request.Role,
            inviterId,
            tokenHash,
            DateTime.UtcNow.Add(InvitationLifetime));

        _dbContext.AddOrganizationInvitation(invitation);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var invitationUrl =
    $"{_frontendOptions.BaseUrl.TrimEnd('/')}" +
    $"/accept-invitation?token=" +
    Uri.EscapeDataString(token);

        var emailBody = $"""
                <h2>You have been invited to ServiCore</h2>

                <p>
                    You have been invited to join an organization on ServiCore
                    as a <strong>{invitation.Role}</strong>.
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
                        Accept Invitation
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
            "You have been invited to ServiCore",
            emailBody,
            cancellationToken);
        return (
            Map(invitation),
            token);
    }

    public async Task AcceptAsync(
    AcceptOrganizationInvitationRequest request,
    CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException(
                "Invitation token is required.",
                nameof(request.Token));

        var tokenHash = _tokenService.HashToken(
            request.Token.Trim());

        var invitation =
            await _dbContext.GetInvitationByTokenHashAsync(
                tokenHash,
                cancellationToken);

        if (invitation is null)
            throw new InvalidOperationException(
                "Invitation is invalid.");

        if (!invitation.IsUsable)
            throw new InvalidOperationException(
                "Invitation is no longer usable.");

        var transaction =
            await _dbContext.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var existingUserId =
                await _identityService.GetUserIdByEmailAsync(
                    invitation.Email,
                    cancellationToken);

            Guid userId;

            if (existingUserId.IsSuccess)
            {
                userId = existingUserId.Value;
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

            var existingRole =
                await _dbContext.GetOrganizationRoleAsync(
                    invitation.OrganizationId,
                    userId,
                    cancellationToken);

            if (existingRole.HasValue)
                throw new InvalidOperationException(
                    "User is already a member of this organization.");

            var organizationMember =
                new OrganizationMember(
                    invitation.OrganizationId,
                    userId,
                    invitation.Role);

            _dbContext.AddOrganizationMember(
                organizationMember);

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
        finally
        {
            await transaction.DisposeAsync();
        }
    }
    public async Task<IReadOnlyList<OrganizationInvitationDto>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var invitations =
            await _dbContext.GetOrganizationInvitationsAsync(
                organizationId,
                cancellationToken);

        return invitations
            .Select(Map)
            .ToList();
    }

    public async Task RevokeAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var invitation =
            await _dbContext.GetOrganizationInvitationAsync(
                organizationId,
                invitationId,
                cancellationToken);

        if (invitation is null)
        {
            throw new KeyNotFoundException(
                "Invitation not found.");
        }

        invitation.Revoke();

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private Guid GetOrganizationId()
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Organization context is required.");
        }

        return _tenantContext.OrganizationId.Value;
    }

    private Guid GetCurrentUserId()
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Authenticated user is required.");
        }

        return _currentUser.UserId.Value;
    }

    private async Task<OrganizationRole?> GetOrganizationRoleAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.GetOrganizationRoleAsync(
            organizationId,
            userId,
            cancellationToken);
    }

    private static OrganizationInvitationDto Map(
        OrganizationInvitation invitation)
    {
        return new OrganizationInvitationDto(
            invitation.Id,
            invitation.OrganizationId,
            invitation.Email,
            invitation.Role.ToString(),
            invitation.CreatedAt,
            invitation.ExpiresAt,
            invitation.IsAccepted,
            invitation.IsRevoked);
    }
}