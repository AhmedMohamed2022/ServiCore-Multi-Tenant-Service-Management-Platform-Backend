using ServiCore.Domain.Common;

namespace ServiCore.Domain.Entities;

public class CustomerInvitation : Entity
{
    public Guid CustomerId { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string Email { get; private set; } = null!;

    public string TokenHash { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? AcceptedAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    private CustomerInvitation()
    {
    }

    public CustomerInvitation(
        Guid customerId,
        Guid organizationId,
        string email,
        string tokenHash,
        DateTime expiresAt)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException(
                "Customer ID is required.",
                nameof(customerId));

        if (organizationId == Guid.Empty)
            throw new ArgumentException(
                "Organization ID is required.",
                nameof(organizationId));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email is required.",
                nameof(email));

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException(
                "Invitation token hash is required.",
                nameof(tokenHash));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException(
                "Invitation expiration must be in the future.",
                nameof(expiresAt));

        Id = Guid.NewGuid();
        CustomerId = customerId;
        OrganizationId = organizationId;
        Email = email.Trim().ToLowerInvariant();
        TokenHash = tokenHash;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired =>
        DateTime.UtcNow >= ExpiresAt;

    public bool IsAccepted =>
        AcceptedAt.HasValue;

    public bool IsRevoked =>
        RevokedAt.HasValue;

    public bool IsUsable =>
        !IsAccepted &&
        !IsRevoked &&
        !IsExpired;

    public void Accept()
    {
        if (IsAccepted)
            throw new InvalidOperationException(
                "Invitation has already been accepted.");

        if (IsRevoked)
            throw new InvalidOperationException(
                "Invitation has been revoked.");

        if (IsExpired)
            throw new InvalidOperationException(
                "Invitation has expired.");

        AcceptedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        if (IsAccepted)
            throw new InvalidOperationException(
                "An accepted invitation cannot be revoked.");

        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
    }
}