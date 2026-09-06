using ServiCore.Application.CustomerInvitations.DTOs;

namespace ServiCore.Application.CustomerInvitations.Interfaces;

public interface ICustomerInvitationService
{
    Task<string> InviteAsync(
        InviteCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task AcceptAsync(
        AcceptCustomerInvitationRequest request,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        Guid invitationId,
        CancellationToken cancellationToken = default);
}