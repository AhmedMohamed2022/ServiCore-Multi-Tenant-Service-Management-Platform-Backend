using ServiCore.Domain.Enums;

namespace ServiCore.Application.Organizations.DTOs;

/// <summary>
/// One organization member available to be added to a team: who they are and
/// what organization role they hold. Role is what lets the Owner/Manager UI
/// tell Managers apart from Agents when picking someone to add — TeamMemberDto
/// (TeamId, UserId, JoinedAt) does not carry a role at all.
/// </summary>
public record OrganizationMemberDto(
    Guid UserId,
    string UserName,
    OrganizationRole Role);
