using ServiCore.Domain.Enums;

namespace ServiCore.Application.OrganizationInvitations.DTOs;

public sealed record InviteOrganizationMemberRequest(
    string Email,
    OrganizationRole Role);
