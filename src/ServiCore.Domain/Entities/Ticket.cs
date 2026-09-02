using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Domain.Common;
using ServiCore.Domain.Enums;

namespace ServiCore.Domain.Entities;

public class Ticket : Entity
{
    public Guid OrganizationId { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid TeamId { get; private set; }

    public Guid? AssignedAgentId { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Title { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public TicketPriority Priority { get; private set; }

    public TicketStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? ResolvedAt { get; private set; }

    public DateTime? ClosedAt { get; private set; }

    private Ticket()
    {
    }

    public Ticket(
        Guid organizationId,
        Guid customerId,
        Guid teamId,
        Guid categoryId,
        string title,
        string description,
        TicketPriority priority)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException(
                "Organization ID is required.",
                nameof(organizationId));

        if (customerId == Guid.Empty)
            throw new ArgumentException(
                "Customer ID is required.",
                nameof(customerId));

        if (teamId == Guid.Empty)
            throw new ArgumentException(
                "Team ID is required.",
                nameof(teamId));

        if (categoryId == Guid.Empty)
            throw new ArgumentException(
                "Category ID is required.",
                nameof(categoryId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(
                "Ticket title is required.",
                nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                "Ticket description is required.",
                nameof(description));

        Id = Guid.NewGuid();

        OrganizationId = organizationId;
        CustomerId = customerId;
        TeamId = teamId;
        CategoryId = categoryId;

        Title = title.Trim();
        Description = description.Trim();

        Priority = priority;
        Status = TicketStatus.New;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
    public void AssignToAgent(Guid agentId)
    {
        if (agentId == Guid.Empty)
            throw new ArgumentException(
                "Agent ID is required.",
                nameof(agentId));

        if (Status == TicketStatus.Closed)
            throw new InvalidOperationException(
                "A closed ticket cannot be assigned.");

        AssignedAgentId = agentId;

        if (Status == TicketStatus.New)
            Status = TicketStatus.Open;

        UpdatedAt = DateTime.UtcNow;
    }

    public void UnassignAgent()
    {
        if (Status == TicketStatus.Closed)
            throw new InvalidOperationException(
                "A closed ticket cannot be unassigned.");

        AssignedAgentId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartProgress()
    {
        if (AssignedAgentId == null)
            throw new InvalidOperationException(
                "A ticket must be assigned to an agent before work can start.");

        if (Status != TicketStatus.Open &&
            Status != TicketStatus.WaitingForCustomer)
            throw new InvalidOperationException(
                "The ticket cannot be moved to InProgress from its current status.");

        Status = TicketStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void WaitForCustomer()
    {
        if (Status != TicketStatus.InProgress)
            throw new InvalidOperationException(
                "Only an in-progress ticket can wait for the customer.");

        Status = TicketStatus.WaitingForCustomer;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resolve()
    {
        if (Status != TicketStatus.InProgress)
            throw new InvalidOperationException(
                "Only an in-progress ticket can be resolved.");

        Status = TicketStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = ResolvedAt.Value;
    }

    public void Close()
    {
        if (Status != TicketStatus.Resolved)
            throw new InvalidOperationException(
                "Only a resolved ticket can be closed.");

        Status = TicketStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        UpdatedAt = ClosedAt.Value;
    }
}
