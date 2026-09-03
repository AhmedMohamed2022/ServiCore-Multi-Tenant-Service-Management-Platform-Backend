using ServiCore.Domain.Enums;

namespace ServiCore.Domain.Entities;

public class OrganizationMember
{
    public Guid OrganizationId { get; private set; }

    public Guid UserId { get; private set; }

    public OrganizationRole Role { get; private set; }

    public DateTime JoinedAt { get; private set; }

    private OrganizationMember()
    {
    }

    public OrganizationMember(
        Guid organizationId,
        Guid userId,
        OrganizationRole role)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Organization ID is required.",
                nameof(organizationId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        }

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentException(
                "A valid organization role is required.",
                nameof(role));
        }

        OrganizationId = organizationId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    public void ChangeRole(OrganizationRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentException(
                "A valid organization role is required.",
                nameof(role));
        }

        Role = role;
    }
}