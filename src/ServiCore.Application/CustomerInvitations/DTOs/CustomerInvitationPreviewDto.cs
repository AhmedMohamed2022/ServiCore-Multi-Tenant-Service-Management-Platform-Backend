namespace ServiCore.Application.CustomerInvitations.DTOs;

/// <summary>
/// What the accept-invitation page needs to know before it shows a form.
/// <see cref="UserExists"/> is true when the invited email already has an
/// account, in which case no new password is collected or applied.
/// </summary>
public sealed record CustomerInvitationPreviewDto(
    string Email,
    bool UserExists);
