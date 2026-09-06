namespace ServiCore.Application.OrganizationInvitations.DTOs;

public sealed record AcceptOrganizationInvitationRequest(
    string Token,
    string Password);