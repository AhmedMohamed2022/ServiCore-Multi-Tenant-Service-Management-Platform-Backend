using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Domain.Common;

namespace ServiCore.Domain.Entities;

public class Category : Entity
{
    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    private Category()
    {
    }

    public Category(
        Guid organizationId,
        string name,
        string? description = null)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException(
                "Organization ID is required.",
                nameof(organizationId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Category name is required.",
                nameof(name));

        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name.Trim();
        Description = description?.Trim();
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
