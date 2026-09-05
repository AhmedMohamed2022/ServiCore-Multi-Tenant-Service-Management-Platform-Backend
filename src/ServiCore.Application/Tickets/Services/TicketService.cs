using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Tickets.DTOs;
using ServiCore.Application.Tickets.Interfaces;
using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;

namespace ServiCore.Application.Tickets.Services;

public sealed class TicketService : ITicketService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public TicketService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<TicketDto> CreateAsync(
        CreateTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var customerBelongsToOrganization =
            await _dbContext.CustomerBelongsToOrganizationAsync(
                organizationId,
                request.CustomerId,
                cancellationToken);

        if (!customerBelongsToOrganization)
            throw new KeyNotFoundException(
                "Customer was not found in the current organization.");

        var teamBelongsToOrganization =
            await _dbContext.TeamBelongsToOrganizationAsync(
                organizationId,
                request.TeamId,
                cancellationToken);

        if (!teamBelongsToOrganization)
            throw new KeyNotFoundException(
                "Team was not found in the current organization.");

        var categoryBelongsToOrganization =
            await _dbContext.CategoryBelongsToOrganizationAsync(
                organizationId,
                request.CategoryId,
                cancellationToken);

        if (!categoryBelongsToOrganization)
            throw new KeyNotFoundException(
                "Category was not found in the current organization.");

        var ticket = new Ticket(
            organizationId,
            request.CustomerId,
            request.TeamId,
            request.CategoryId,
            request.Title,
            request.Description,
            request.Priority);

        _dbContext.AddTicket(ticket);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }

    public async Task<IReadOnlyList<TicketDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var tickets = await _dbContext.GetTicketsAsync(
            organizationId,
            cancellationToken);

        return tickets
            .Select(Map)
            .ToList();
    }

    public async Task<TicketDto> GetByIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var ticket = await _dbContext.GetTicketAsync(
            organizationId,
            ticketId,
            cancellationToken);

        if (ticket is null)
            throw new KeyNotFoundException(
                "Ticket was not found.");

        return Map(ticket);
    }

    public async Task<TicketDto> UpdateAsync(
        Guid ticketId,
        UpdateTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var ticket = await _dbContext.GetTicketAsync(
            organizationId,
            ticketId,
            cancellationToken);

        if (ticket is null)
            throw new KeyNotFoundException(
                "Ticket was not found.");

        var categoryBelongsToOrganization =
            await _dbContext.CategoryBelongsToOrganizationAsync(
                organizationId,
                request.CategoryId,
                cancellationToken);

        if (!categoryBelongsToOrganization)
            throw new KeyNotFoundException(
                "Category was not found in the current organization.");

        ticket.Update(
            request.Title,
            request.Description,
            request.CategoryId,
            request.Priority);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }

    public async Task<TicketDto> OpenAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var ticket = await _dbContext.GetTicketAsync(
            organizationId,
            ticketId,
            cancellationToken);

        if (ticket is null)
            throw new KeyNotFoundException(
                "Ticket was not found.");

        ticket.Open();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }

    private Guid GetOrganizationId()
    {
        return _tenantContext.OrganizationId
            ?? throw new InvalidOperationException(
                "No organization context is available.");
    }

    public async Task<TicketDto> AssignAsync(
      Guid ticketId,
      Guid agentId,
      CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var ticket = await GetTicketOrThrowAsync(
            organizationId,
            ticketId,
            cancellationToken);

        var agentOrganizationRole =
            await _dbContext.GetOrganizationRoleAsync(
                organizationId,
                agentId,
                cancellationToken);

        if (agentOrganizationRole != OrganizationRole.Agent)
            throw new KeyNotFoundException(
                "The selected user is not an agent in the current organization.");

        var agentIsTeamMember =
            await _dbContext.TeamMemberExistsAsync(
                ticket.TeamId,
                agentId,
                cancellationToken);

        if (!agentIsTeamMember)
            throw new KeyNotFoundException(
                "The selected agent is not a member of the ticket's team.");

        ticket.AssignToAgent(agentId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }

    public async Task<TicketDto> UnassignAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var ticket = await GetTicketOrThrowAsync(
            organizationId,
            ticketId,
            cancellationToken);

        ticket.UnassignAgent();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }

    public async Task<TicketDto> StartProgressAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await GetTicketForAssignedAgentAsync(
            ticketId,
            cancellationToken);

        ticket.StartProgress();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }

    public async Task<TicketDto> WaitForCustomerAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await GetTicketForAssignedAgentAsync(
            ticketId,
            cancellationToken);

        ticket.WaitForCustomer();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }

    public async Task<TicketDto> ResolveAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await GetTicketForAssignedAgentAsync(
            ticketId,
            cancellationToken);

        ticket.Resolve();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }

    public async Task<TicketDto> CloseAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var ticket = await GetTicketOrThrowAsync(
            organizationId,
            ticketId,
            cancellationToken);

        ticket.Close();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }
    private async Task<Ticket> GetTicketForAssignedAgentAsync(
            Guid ticketId,
            CancellationToken cancellationToken)
    {
        var organizationId = GetOrganizationId();
        var currentUserId = GetCurrentUserId();

        var ticket = await GetTicketOrThrowAsync(
            organizationId,
            ticketId,
            cancellationToken);

        if (ticket.AssignedAgentId != currentUserId)
            throw new UnauthorizedAccessException(
                "You are not the assigned agent for this ticket.");

        var role = await _dbContext.GetOrganizationRoleAsync(
            organizationId,
            currentUserId,
            cancellationToken);

        if (role != OrganizationRole.Agent)
            throw new UnauthorizedAccessException(
                "Only an assigned agent can perform this operation.");

        return ticket;
    }

    private async Task<Ticket> GetTicketOrThrowAsync(
        Guid organizationId,
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var ticket = await _dbContext.GetTicketAsync(
            organizationId,
            ticketId,
            cancellationToken);

        if (ticket is null)
            throw new KeyNotFoundException(
                "Ticket was not found.");

        return ticket;
    }

    private Guid GetCurrentUserId()
    {
        return _currentUser.UserId
            ?? throw new UnauthorizedAccessException(
                "The current user is not authenticated.");
    }

    private static TicketDto Map(Ticket ticket)
    {
        return new TicketDto(
            ticket.Id,
            ticket.OrganizationId,
            ticket.CustomerId,
            ticket.TeamId,
            ticket.AssignedAgentId,
            ticket.CategoryId,
            ticket.Title,
            ticket.Description,
            ticket.Priority,
            ticket.Status,
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.ResolvedAt,
            ticket.ClosedAt);
    }
}