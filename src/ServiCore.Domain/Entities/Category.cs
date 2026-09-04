using ServiCore.Domain.Common;

namespace ServiCore.Domain.Entities;

public class Category : Entity
{
    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

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

        var now = DateTime.UtcNow;

        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name.Trim();
        Description = description?.Trim();
        IsActive = true;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public void Update(
        string name,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Category name is required.",
                nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}