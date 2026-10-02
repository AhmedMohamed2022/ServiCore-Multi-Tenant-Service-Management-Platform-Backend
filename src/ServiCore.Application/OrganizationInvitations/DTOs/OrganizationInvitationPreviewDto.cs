namespace ServiCore.Application.OrganizationInvitations.DTOs;

/// <summary>
/// What the accept-invitation page needs to know before it shows a form.
/// <see cref="UserExists"/> tells the page whether the invited email already
/// has an account (so it should ask the person to sign in with their existing
/// password instead of asking them to choose a new one).
/// </summary>
public sealed record OrganizationInvitationPreviewDto(
    string Email,
    string Role,
    bool UserExists);
