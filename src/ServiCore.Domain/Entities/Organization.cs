using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Domain.Common;

namespace ServiCore.Domain.Entities;

public class Organization : Entity
{
    public string Name { get; private set; }

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
}
