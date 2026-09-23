using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Notifications.DTOs;
using ServiCore.Application.Notifications.Interfaces;
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
    private readonly INotificationService _notificationService;

    public TicketService(IApplicationDbContext dbContext, ITenantContext tenantContext, ICurrentUser currentUser, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<TicketDto> CreateAsync(
        CreateTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var currentUserId = GetCurrentUserId();

        var customerBelongsToOrganization =
            await _dbContext.CustomerBelongsToOrganizationAsync(
                organizationId,
                request.CustomerId,
                cancellationToken);

        if (!customerBelongsToOrganization)
        {
            throw new KeyNotFoundException(
                "Customer was not found in the current organization.");
        }

        await EnsureCanCreateStaffTicketAsync(
            organizationId,
            currentUserId,
            cancellationToken);

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

    public async Task<TicketDto> CreateForCustomerAsync(
        CreateCustomerTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var currentUserId = GetCurrentUserId();

        var customer =
            await _dbContext.GetCustomerForUserAsync(
                organizationId,
                currentUserId,
                cancellationToken);

        if (customer is null)
        {
            throw new UnauthorizedAccessException(
                "The current user is not an active customer in this organization.");
        }

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
            customer.Id,
            null,
            request.CategoryId,
            request.Title,
            request.Description,
            request.Priority);

        _dbContext.AddTicket(ticket);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var triageUserIds =
            await _dbContext.GetOrganizationTriageUserIdsAsync(
                organizationId,
                cancellationToken);

        foreach (var userId in triageUserIds)
        {
            await _notificationService.CreateAsync(
                new CreateNotificationRequest(
                    UserId: userId,
                    Type: NotificationType.TicketCreated,
                    Title: "New customer ticket",
                    Message: $"Customer \"{customer.Name}\" submitted ticket \"{ticket.Title}\" and it needs triage.",
                    RelatedEntityId: ticket.Id),
                cancellationToken);
        }

        return Map(ticket);
    }

    public async Task<IReadOnlyList<TicketDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var currentUserId = GetCurrentUserId();

        IReadOnlyList<Ticket> tickets;

        var role = await _dbContext.GetOrganizationRoleAsync(
            organizationId,
            currentUserId,
            cancellationToken);

        if (!role.HasValue)
        {
            tickets = await _dbContext.GetCustomerTicketsAsync(
                organizationId,
                currentUserId,
                cancellationToken);
        }
        else
        {
            tickets = role.Value switch
            {
                OrganizationRole.Owner =>
                    await _dbContext.GetTicketsAsync(
                        organizationId,
                        cancellationToken),

                OrganizationRole.Manager =>
                    await _dbContext.GetManagerTicketsAsync(
                        organizationId,
                        currentUserId,
                        cancellationToken),

                OrganizationRole.Agent =>
                    await _dbContext.GetAgentTicketsAsync(
                        organizationId,
                        currentUserId,
                        cancellationToken),

                _ => Array.Empty<Ticket>()
            };
        }

        return tickets
            .Select(Map)
            .ToList();
    }

    public async Task<TicketDto> GetByIdAsync(
    Guid ticketId,
    CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var currentUserId = GetCurrentUserId();

        var role = await _dbContext.GetOrganizationRoleAsync(
            organizationId,
            currentUserId,
            cancellationToken);

        Ticket? ticket;

        if (!role.HasValue)
        {
            ticket = await _dbContext.GetCustomerTicketAsync(
                organizationId,
                ticketId,
                currentUserId,
                cancellationToken);
        }
        else
        {
            ticket = role.Value switch
            {
                OrganizationRole.Owner =>
                    await _dbContext.GetTicketAsync(
                        organizationId,
                        ticketId,
                        cancellationToken),

                OrganizationRole.Manager =>
                    await _dbContext.GetManagerTicketAsync(
                        organizationId,
                        currentUserId,
                        ticketId,
                        cancellationToken),

                OrganizationRole.Agent =>
                    await _dbContext.GetAgentTicketAsync(
                        organizationId,
                        currentUserId,
                        ticketId,
                        cancellationToken),

                _ => null
            };
        }

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

        await EnsureCanManageTicketAsync(
            organizationId,
            ticket,
            cancellationToken);

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

        await EnsureCanManageTicketAsync(
            organizationId,
            ticket,
            cancellationToken);

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

    public async Task<TicketDto> AssignTeamAsync(
     Guid ticketId,
     Guid teamId,
     CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        var currentUserId = GetCurrentUserId();

        var ticket = await GetTicketOrThrowAsync(
            organizationId,
            ticketId,
            cancellationToken);

        await EnsureCanManageTicketAsync(
            organizationId,
            ticket,
            cancellationToken);

        var teamBelongsToOrganization =
            await _dbContext.TeamBelongsToOrganizationAsync(
                organizationId,
                teamId,
                cancellationToken);

        if (!teamBelongsToOrganization)
        {
            throw new KeyNotFoundException(
                "Team was not found in the current organization.");
        }

        var role = await _dbContext.GetOrganizationRoleAsync(
            organizationId,
            currentUserId,
            cancellationToken);

        if (role == OrganizationRole.Manager)
        {
            var managerBelongsToTargetTeam =
                await _dbContext.TeamMemberExistsAsync(
                    teamId,
                    currentUserId,
                    cancellationToken);

            if (!managerBelongsToTargetTeam)
            {
                throw new UnauthorizedAccessException(
                    "You can only assign tickets to teams you manage.");
            }
        }

        ticket.AssignToTeam(teamId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var teamManagerIds =
            await _dbContext.GetTeamManagerUserIdsAsync(
                organizationId,
                teamId,
                cancellationToken);

        foreach (var managerId in teamManagerIds)
        {
            await _notificationService.CreateAsync(
                new CreateNotificationRequest(
                    UserId: managerId,
                    Type: NotificationType.TicketTeamAssigned,
                    Title: "Ticket assigned to your team",
                    Message:
                        $"Ticket \"{ticket.Title}\" has been assigned to your team.",
                    RelatedEntityId: ticket.Id),
                cancellationToken);
        }

        return Map(ticket);
    }
    public async Task<TicketDto> UnassignTeamAsync(
    Guid ticketId,
    CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();

        var ticket = await GetTicketOrThrowAsync(
            organizationId,
            ticketId,
            cancellationToken);

        await EnsureCanManageTicketAsync(
            organizationId,
            ticket,
            cancellationToken);

        if (!ticket.TeamId.HasValue)
        {
            throw new InvalidOperationException(
                "The ticket is not assigned to a team.");
        }

        ticket.UnassignTeam();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(ticket);
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

        await EnsureCanManageTicketAsync(
            organizationId,
            ticket,
            cancellationToken);

        var agentOrganizationRole =
            await _dbContext.GetOrganizationRoleAsync(
                organizationId,
                agentId,
                cancellationToken);

        if (agentOrganizationRole != OrganizationRole.Agent)
            throw new KeyNotFoundException(
                "The selected user is not an agent in the current organization.");

        if (!ticket.TeamId.HasValue)
            throw new InvalidOperationException(
                "A ticket must be assigned to a team before an agent can be assigned.");

        var agentIsTeamMember =
            await _dbContext.TeamMemberExistsAsync(
                ticket.TeamId.Value,
                agentId,
                cancellationToken);

        if (!agentIsTeamMember)
            throw new KeyNotFoundException(
                "The selected agent is not a member of the ticket's team.");

        ticket.AssignToAgent(agentId);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _notificationService.CreateAsync(
            new CreateNotificationRequest(
                UserId: agentId,
                Type: NotificationType.TicketAssigned,
                Title: "Ticket assigned",
                Message: $"Ticket \"{ticket.Title}\" has been assigned to you.",
                RelatedEntityId: ticket.Id),
            cancellationToken);

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

        await EnsureCanManageTicketAsync(
            organizationId,
            ticket,
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
        var organizationId = GetOrganizationId();

        var ticket = await GetTicketForAssignedAgentAsync(
            ticketId,
            cancellationToken);

        ticket.Resolve();

        await _dbContext.SaveChangesAsync(cancellationToken);
        var customerUserId =
    await _dbContext.GetTicketCustomerUserIdAsync(
        organizationId,
        ticket.Id,
        cancellationToken);

        if (customerUserId.HasValue)
        {
            await _notificationService.CreateAsync(
                new CreateNotificationRequest(
                    UserId: customerUserId.Value,
                    Type: NotificationType.TicketResolved,
                    Title: "Ticket resolved",
                    Message: $"Your ticket \"{ticket.Title}\" has been resolved.",
                    RelatedEntityId: ticket.Id),
                cancellationToken);
        }

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

        await EnsureCanManageTicketAsync(
            organizationId,
            ticket,
            cancellationToken);

        ticket.Close();

        await _dbContext.SaveChangesAsync(cancellationToken);
        var customerUserId =
    await _dbContext.GetTicketCustomerUserIdAsync(
        organizationId,
        ticket.Id,
        cancellationToken);

        if (customerUserId.HasValue)
        {
            await _notificationService.CreateAsync(
                new CreateNotificationRequest(
                    UserId: customerUserId.Value,
                    Type: NotificationType.TicketClosed,
                    Title: "Ticket closed",
                    Message: $"Your ticket \"{ticket.Title}\" has been closed.",
                    RelatedEntityId: ticket.Id),
                cancellationToken);
        }

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

    private async Task EnsureCanReadTicketAsync(
        Guid organizationId,
        Ticket ticket,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        var role = await _dbContext.GetOrganizationRoleAsync(
            organizationId,
            currentUserId,
            cancellationToken);

        if (role == OrganizationRole.Owner)
            return;

        if (role == OrganizationRole.Manager)
        {
            if (!ticket.TeamId.HasValue)
                return;

            var isTeamMember =
                await _dbContext.TeamMemberExistsAsync(
                    ticket.TeamId.Value,
                    currentUserId,
                    cancellationToken);

            if (isTeamMember)
                return;

            throw new UnauthorizedAccessException(
                "You can only view tickets assigned to your team.");
        }

        if (role == OrganizationRole.Agent &&
            ticket.AssignedAgentId == currentUserId)
            return;

        throw new UnauthorizedAccessException(
            "You are not allowed to view this ticket.");
    }

    private async Task EnsureCanCreateStaffTicketAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.GetOrganizationRoleAsync(
            organizationId,
            userId,
            cancellationToken);

        if (role != OrganizationRole.Owner &&
            role != OrganizationRole.Manager)
        {
            throw new UnauthorizedAccessException(
                "Only organization owners and managers can create staff tickets.");
        }
    }

    private async Task EnsureCanManageTicketAsync(
        Guid organizationId,
        Ticket ticket,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        var role = await _dbContext.GetOrganizationRoleAsync(
            organizationId,
            currentUserId,
            cancellationToken);

        if (role == OrganizationRole.Owner)
            return;

        if (role != OrganizationRole.Manager)
        {
            throw new UnauthorizedAccessException(
                "Only organization owners and managers can manage tickets.");
        }

        if (!ticket.TeamId.HasValue)
            return;

        var isTeamMember =
            await _dbContext.TeamMemberExistsAsync(
                ticket.TeamId.Value,
                currentUserId,
                cancellationToken);

        if (!isTeamMember)
        {
            throw new UnauthorizedAccessException(
                "You can only manage tickets assigned to your team.");
        }
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
    private async Task<bool> IsCustomerAsync(
    Guid organizationId,
    CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var organizationRole =
            await _dbContext.GetOrganizationRoleAsync(
                organizationId,
                userId,
                cancellationToken);

        return !organizationRole.HasValue;
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