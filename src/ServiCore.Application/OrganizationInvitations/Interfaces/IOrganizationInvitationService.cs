using ServiCore.Application.OrganizationInvitations.DTOs;

namespace ServiCore.Application.OrganizationInvitations.Interfaces;

public interface IOrganizationInvitationService
{
    Task<(
        OrganizationInvitationDto Invitation,
        string Token)> InviteAsync(
        InviteOrganizationMemberRequest request,
        CancellationToken cancellationToken = default);

    Task AcceptAsync(
        AcceptOrganizationInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationInvitationDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default);
}