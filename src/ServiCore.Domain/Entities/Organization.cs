using ServiCore.Domain.Common;

namespace ServiCore.Domain.Entities;

public class Organization : Entity
{
    public Guid? OwnerUserId { get; private set; }

    public string Name { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    private Organization()
    {
    }

    public Organization(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Organization name is required.",
                nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void SetOwner(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));

        OwnerUserId = userId;
    }
}