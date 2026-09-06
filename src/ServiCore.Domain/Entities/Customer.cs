using ServiCore.Domain.Common;

namespace ServiCore.Domain.Entities;

public class Customer : Entity
{
    public Guid OrganizationId { get; private set; }

    public Guid? UserId { get; private set; }

    public string Name { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string? PhoneNumber { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private Customer()
    {
    }

    public Customer(
        Guid organizationId,
        string name,
        string email,
        string? phoneNumber = null)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException(
                "Organization ID is required.",
                nameof(organizationId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Customer name is required.",
                nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Customer email is required.",
                nameof(email));

        var now = DateTime.UtcNow;

        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = phoneNumber?.Trim();
        IsActive = true;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public void Update(
        string name,
        string email,
        string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Customer name is required.",
                nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Customer email is required.",
                nameof(email));

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = phoneNumber?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        if (UserId.HasValue && UserId.Value != userId)
            throw new InvalidOperationException(
                "Customer is already linked to another user.");

        UserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }
    public bool IsLinked =>
    UserId.HasValue;
}