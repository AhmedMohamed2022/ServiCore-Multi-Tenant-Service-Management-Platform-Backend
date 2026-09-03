using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Domain.Common;

namespace ServiCore.Domain.Entities;

public class Team : Entity
{
    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Team()
    {
    }

    public Team(Guid organizationId,string name,string? description = null)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException(
                "Organization ID is required.",
                nameof(organizationId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Team name is required.",
                nameof(name));

        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name.Trim();
        Description = description?.Trim();
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }
    public void Update(string name,string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Team name is required.",
                nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
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