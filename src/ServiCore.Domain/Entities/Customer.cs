using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Domain.Common;

namespace ServiCore.Domain.Entities;

public class Customer : Entity
{
    public Guid OrganizationId { get; private set; }

    public Guid? UserId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? PhoneNumber { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Customer()
    {
    }

    public Customer(
        Guid organizationId,
        string name,
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

        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name.Trim();
        PhoneNumber = phoneNumber?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void LinkUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));

        UserId = userId;
    }
}
