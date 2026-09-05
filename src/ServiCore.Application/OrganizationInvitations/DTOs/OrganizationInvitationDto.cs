namespace ServiCore.Application.OrganizationInvitations.DTOs;

public sealed record OrganizationInvitationDto(
    Guid Id,
    Guid OrganizationId,
    string Email,
    string Role,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsAccepted,
    bool IsRevoked);