namespace ServiCore.Application.Customers.DTOs;

// One row per organization the currently-authenticated user has a linked
// (accepted-invitation) Customer record in. A user can in principle be a
// customer of more than one organization, so this is a list rather than a
// single value — same shape decision as OrganizationDto for staff "mine".
public sealed record CustomerMembershipDto(
    Guid CustomerId,
    Guid OrganizationId,
    string OrganizationName);
