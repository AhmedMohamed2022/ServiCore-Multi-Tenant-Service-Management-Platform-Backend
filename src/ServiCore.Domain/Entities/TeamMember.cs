using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiCore.Domain.Entities;

public class TeamMember
{
    public Guid TeamId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime JoinedAt { get; private set; }

    private TeamMember()
    {
    }

    public TeamMember(Guid teamId, Guid userId)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException(
                "Team ID is required.",
                nameof(teamId));

        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));

        TeamId = teamId;
        UserId = userId;
        JoinedAt = DateTime.UtcNow;
    }
}
