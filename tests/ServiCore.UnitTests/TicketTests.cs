using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;

namespace ServiCore.UnitTests;

public sealed class TicketTests
{
    private static Ticket CreateTicket()
    {
        return new Ticket(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  Cannot login  ",
            "  Customer cannot access the portal.  ",
            TicketPriority.High);
    }

    [Fact]
    public void Constructor_ShouldCreateNewTicket()
    {
        var ticket = CreateTicket();

        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.Equal(TicketStatus.New, ticket.Status);
        Assert.Equal(TicketPriority.High, ticket.Priority);
        Assert.Equal("Cannot login", ticket.Title);
        Assert.Equal("Customer cannot access the portal.", ticket.Description);
        Assert.Null(ticket.AssignedAgentId);
        Assert.Null(ticket.ResolvedAt);
        Assert.Null(ticket.ClosedAt);
    }

    [Fact]
    public void Open_ShouldMoveNewTicketToOpen()
    {
        var ticket = CreateTicket();

        ticket.Open();

        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public void AssignToAgent_ShouldAssignAndOpenNewTicket()
    {
        var ticket = CreateTicket();
        var agentId = Guid.NewGuid();

        ticket.AssignToAgent(agentId);

        Assert.Equal(agentId, ticket.AssignedAgentId);
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public void StartProgress_ShouldRequireAssignedAgent()
    {
        var ticket = CreateTicket();
        ticket.Open();

        var exception = Assert.Throws<InvalidOperationException>(
            () => ticket.StartProgress());

        Assert.Contains("assigned to an agent", exception.Message);
    }

    [Fact]
    public void StartProgress_ShouldMoveOpenAssignedTicketToInProgress()
    {
        var ticket = CreateTicket();
        ticket.AssignToAgent(Guid.NewGuid());

        ticket.StartProgress();

        Assert.Equal(TicketStatus.InProgress, ticket.Status);
    }

    [Fact]
    public void WaitForCustomer_ShouldMoveInProgressTicket()
    {
        var ticket = CreateTicket();
        ticket.AssignToAgent(Guid.NewGuid());
        ticket.StartProgress();

        ticket.WaitForCustomer();

        Assert.Equal(TicketStatus.WaitingForCustomer, ticket.Status);
    }

    [Fact]
    public void StartProgress_FromWaitingForCustomer_ShouldResumeWork()
    {
        var ticket = CreateTicket();
        ticket.AssignToAgent(Guid.NewGuid());
        ticket.StartProgress();
        ticket.WaitForCustomer();

        ticket.StartProgress();

        Assert.Equal(TicketStatus.InProgress, ticket.Status);
    }

    [Fact]
    public void Resolve_ShouldSetResolvedStatusAndTimestamp()
    {
        var ticket = CreateTicket();
        ticket.AssignToAgent(Guid.NewGuid());
        ticket.StartProgress();

        ticket.Resolve();

        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.NotNull(ticket.ResolvedAt);
        Assert.Null(ticket.ClosedAt);
    }

    [Fact]
    public void Close_ShouldSetClosedStatusAndTimestamp()
    {
        var ticket = CreateTicket();
        ticket.AssignToAgent(Guid.NewGuid());
        ticket.StartProgress();
        ticket.Resolve();

        ticket.Close();

        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.NotNull(ticket.ClosedAt);
    }

    [Fact]
    public void Resolve_FromNew_ShouldThrow()
    {
        var ticket = CreateTicket();

        Assert.Throws<InvalidOperationException>(() => ticket.Resolve());
    }

    [Fact]
    public void Close_FromInProgress_ShouldThrow()
    {
        var ticket = CreateTicket();
        ticket.AssignToAgent(Guid.NewGuid());
        ticket.StartProgress();

        Assert.Throws<InvalidOperationException>(() => ticket.Close());
    }

    [Fact]
    public void ClosedTicket_ShouldNotBeUpdated()
    {
        var ticket = CreateTicket();
        ticket.AssignToAgent(Guid.NewGuid());
        ticket.StartProgress();
        ticket.Resolve();
        ticket.Close();

        Assert.Throws<InvalidOperationException>(() => ticket.Update(
            "Updated title",
            "Updated description",
            Guid.NewGuid(),
            TicketPriority.Low));
    }

    [Fact]
    public void ClosedTicket_ShouldNotBeAssignedOrUnassigned()
    {
        var ticket = CreateTicket();
        ticket.AssignToAgent(Guid.NewGuid());
        ticket.StartProgress();
        ticket.Resolve();
        ticket.Close();

        Assert.Throws<InvalidOperationException>(
            () => ticket.AssignToAgent(Guid.NewGuid()));
        Assert.Throws<InvalidOperationException>(
            () => ticket.UnassignAgent());
    }
}
