using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Domain.Common;

namespace ServiCore.Domain.Entities;

public class TicketComment : Entity
{
    public Guid TicketId { get; private set; }

    public Guid AuthorUserId { get; private set; }

    public string Content { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    private TicketComment()
    {
    }

    public TicketComment(
        Guid ticketId,
        Guid authorUserId,
        string content)
    {
        if (ticketId == Guid.Empty)
            throw new ArgumentException(
                "Ticket ID is required.",
                nameof(ticketId));

        if (authorUserId == Guid.Empty)
            throw new ArgumentException(
                "Author user ID is required.",
                nameof(authorUserId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException(
                "Comment content is required.",
                nameof(content));

        Id = Guid.NewGuid();

        TicketId = ticketId;
        AuthorUserId = authorUserId;
        Content = content.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}
