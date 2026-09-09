using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Reporting.Queries;
using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;
using ServiCore.Infrastructure.Identity;

namespace ServiCore.Infrastructure.Persistence;

public class ServiCoreDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>,
      IApplicationDbContext
{
    public ServiCoreDbContext(
        DbContextOptions<ServiCoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationInvitation> OrganizationInvitations => Set<OrganizationInvitation>();

    public DbSet<OrganizationMember> OrganizationMembers
        => Set<OrganizationMember>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerInvitation> CustomerInvitations => Set<CustomerInvitation>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketComment> TicketComments
        => Set<TicketComment>();

    public void AddOrganization(Organization organization)
    {
        Organizations.Add(organization);
    }

    public void AddOrganizationMember(
    OrganizationMember member)
    {
        OrganizationMembers.Add(member);
    }

    public void AddTeam(Team team)
    {
        Teams.Add(team);
    }
    public async Task<IReadOnlyList<Team>> GetTeamsAsync(
    Guid organizationId,
    CancellationToken cancellationToken = default)
    {
        return await Teams
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Team?> GetTeamAsync(
        Guid organizationId,
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        return await Teams
            .SingleOrDefaultAsync(
                x =>
                    x.Id == teamId &&
                    x.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IApplicationTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction = await Database.BeginTransactionAsync(
            cancellationToken);

        return new ApplicationTransaction(transaction);
    }
    public void AddTeamMember(TeamMember member)
    {
        TeamMembers.Add(member);
    }

    public async Task<bool> OrganizationMemberExistsAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await OrganizationMembers
            .AnyAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.UserId == userId,
                cancellationToken);
    }

    public async Task<bool> TeamMemberExistsAsync(
        Guid teamId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await TeamMembers
            .AnyAsync(
                x =>
                    x.TeamId == teamId &&
                    x.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<TeamMember>>
        GetTeamMembersAsync(
            Guid organizationId,
            Guid teamId,
            CancellationToken cancellationToken = default)
    {
        return await TeamMembers
            .Where(x =>
                x.TeamId == teamId &&
                Teams.Any(team =>
                    team.Id == x.TeamId &&
                    team.OrganizationId == organizationId))
            .OrderBy(x => x.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TeamMember?> GetTeamMemberAsync(
        Guid organizationId,
        Guid teamId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await TeamMembers
            .Where(x =>
                x.TeamId == teamId &&
                x.UserId == userId &&
                Teams.Any(team =>
                    team.Id == x.TeamId &&
                    team.OrganizationId == organizationId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public void RemoveTeamMember(TeamMember member)
    {
        TeamMembers.Remove(member);
    }
    public void AddCustomer(Customer customer)
    {
        Customers.Add(customer);
    }

    public async Task<IReadOnlyList<Customer>> GetCustomersAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await Customers
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetCustomerAsync(
        Guid organizationId,
        Guid customerId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = Customers.Where(x =>
            x.OrganizationId == organizationId &&
            x.Id == customerId);

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CustomerEmailExistsAsync(
        Guid organizationId,
        string email,
        Guid? excludingCustomerId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        return await Customers.AnyAsync(
            x =>
                x.OrganizationId == organizationId &&
                x.Email == normalizedEmail &&
                (!excludingCustomerId.HasValue ||
                 x.Id != excludingCustomerId.Value),
            cancellationToken);
    }
    public async Task<IReadOnlyList<Ticket>> GetCustomerTicketsAsync(
    Guid organizationId,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await Tickets
            .AsNoTracking()
            .Where(ticket =>
                ticket.OrganizationId == organizationId &&
                Customers.Any(customer =>
                    customer.Id == ticket.CustomerId &&
                    customer.OrganizationId == organizationId &&
                    customer.UserId == userId &&
                    customer.IsActive))
            .OrderByDescending(ticket => ticket.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<Ticket?> GetCustomerTicketAsync(
    Guid organizationId,
    Guid ticketId,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await Tickets
            .Where(ticket =>
                ticket.Id == ticketId &&
                ticket.OrganizationId == organizationId &&
                Customers.Any(customer =>
                    customer.Id == ticket.CustomerId &&
                    customer.OrganizationId == organizationId &&
                    customer.UserId == userId &&
                    customer.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }
    public async Task<bool> CustomerBelongsToUserAsync(
    Guid organizationId,
    Guid customerId,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await Customers
            .AnyAsync(
                customer =>
                    customer.Id == customerId &&
                    customer.OrganizationId == organizationId &&
                    customer.UserId == userId &&
                    customer.IsActive,
                cancellationToken);
    }
    public void AddCategory(Category category)
    {
        Categories.Add(category);
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await Categories
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetCategoryAsync(
        Guid organizationId,
        Guid categoryId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = Categories.Where(x =>
            x.OrganizationId == organizationId &&
            x.Id == categoryId);

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query.SingleOrDefaultAsync(
            cancellationToken);
    }

    public async Task<bool> CategoryNameExistsAsync(
        Guid organizationId,
        string name,
        Guid? excludingCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();

        return await Categories.AnyAsync(
            x =>
                x.OrganizationId == organizationId &&
                x.Name == normalizedName &&
                (!excludingCategoryId.HasValue ||
                 x.Id != excludingCategoryId.Value),
            cancellationToken);
    }
    public void AddTicket(Ticket ticket)
    {
        Tickets.Add(ticket);
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await Tickets
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ticket?> GetTicketAsync(
        Guid organizationId,
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await Tickets
            .FirstOrDefaultAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.Id == ticketId,
                cancellationToken);
    }

    public async Task<bool> CustomerBelongsToOrganizationAsync(
        Guid organizationId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await Customers
            .AnyAsync(
                x =>
                    x.Id == customerId &&
                    x.OrganizationId == organizationId &&
                    x.IsActive,
                cancellationToken);
    }

    public async Task<bool> TeamBelongsToOrganizationAsync(
        Guid organizationId,
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        return await Teams
            .AnyAsync(
                x =>
                    x.Id == teamId &&
                    x.OrganizationId == organizationId &&
                    x.IsActive,
                cancellationToken);
    }

    public async Task<bool> CategoryBelongsToOrganizationAsync(
        Guid organizationId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await Categories
            .AnyAsync(
                x =>
                    x.Id == categoryId &&
                    x.OrganizationId == organizationId &&
                    x.IsActive,
                cancellationToken);
    }
    public async Task<OrganizationRole?> GetOrganizationRoleAsync(
    Guid organizationId,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await OrganizationMembers
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.UserId == userId)
            .Select(x => (OrganizationRole?)x.Role)
            .SingleOrDefaultAsync(cancellationToken);
    }
    public void AddTicketComment(TicketComment comment)
    {
        TicketComments.Add(comment);
    }
    public async Task<IReadOnlyList<TicketComment>> GetTicketCommentsAsync(
    Guid organizationId,
    Guid ticketId,
    CancellationToken cancellationToken = default)
    {
        return await TicketComments
            .AsNoTracking()
            .Where(comment =>
                comment.TicketId == ticketId &&
                Tickets.Any(ticket =>
                    ticket.Id == comment.TicketId &&
                    ticket.OrganizationId == organizationId))
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<bool> CustomerOwnsTicketAsync(
    Guid organizationId,
    Guid ticketId,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await Tickets
            .AnyAsync(
                ticket =>
                    ticket.Id == ticketId &&
                    ticket.OrganizationId == organizationId &&
                    Customers.Any(customer =>
                        customer.Id == ticket.CustomerId &&
                        customer.OrganizationId == organizationId &&
                        customer.UserId == userId),
                cancellationToken);
    }
    public void AddOrganizationInvitation(
    OrganizationInvitation invitation)
    {
        OrganizationInvitations.Add(invitation);
    }
    public async Task<OrganizationInvitation?> GetInvitationByTokenHashAsync(
    string tokenHash,
    CancellationToken cancellationToken = default)
    {
        return await OrganizationInvitations
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }
    public async Task<bool> PendingInvitationExistsAsync(
    Guid organizationId,
    string email,
    CancellationToken cancellationToken = default)
    {
        email = email.Trim().ToLowerInvariant();

        return await OrganizationInvitations
            .AnyAsync(
                x =>
                    x.OrganizationId == organizationId &&
                    x.Email == email &&
                    x.AcceptedAt == null &&
                    x.RevokedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }
    public async Task<IReadOnlyList<OrganizationInvitation>>
    GetOrganizationInvitationsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await OrganizationInvitations
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<OrganizationInvitation?>
    GetOrganizationInvitationAsync(
        Guid organizationId,
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        return await OrganizationInvitations
            .SingleOrDefaultAsync(
                x =>
                    x.Id == invitationId &&
                    x.OrganizationId == organizationId,
                cancellationToken);
    }

    public void AddCustomerInvitation(
    CustomerInvitation invitation)
    {
        CustomerInvitations.Add(invitation);
    }

    public async Task<CustomerInvitation?>
        GetCustomerInvitationByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
    {
        return await CustomerInvitations
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task<bool> PendingCustomerInvitationExistsAsync(
        Guid organizationId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await CustomerInvitations.AnyAsync(
            x =>
                x.OrganizationId == organizationId &&
                x.CustomerId == customerId &&
                x.AcceptedAt == null &&
                x.RevokedAt == null &&
                x.ExpiresAt > DateTime.UtcNow,
            cancellationToken);
    }

    public async Task<CustomerInvitation?>
        GetCustomerInvitationAsync(
            Guid organizationId,
            Guid invitationId,
            CancellationToken cancellationToken = default)
    {
        return await CustomerInvitations
            .SingleOrDefaultAsync(
                x =>
                    x.Id == invitationId &&
                    x.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<Customer?>
        GetCustomerForOrganizationAsync(
            Guid organizationId,
            Guid customerId,
            CancellationToken cancellationToken = default)
    {
        return await Customers
            .SingleOrDefaultAsync(
                x =>
                    x.Id == customerId &&
                    x.OrganizationId == organizationId,
                cancellationToken);
    }
    public void AddNotification(Notification notification)
    {
        Notifications.Add(notification);
    }
    public async Task<IReadOnlyList<Notification>> GetNotificationsAsync(
    Guid organizationId,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.OrganizationId == organizationId &&
                notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<Notification?> GetNotificationAsync(
    Guid organizationId,
    Guid userId,
    Guid notificationId,
    CancellationToken cancellationToken = default)
    {
        return await Notifications
            .FirstOrDefaultAsync(
                notification =>
                    notification.Id == notificationId &&
                    notification.OrganizationId == organizationId &&
                    notification.UserId == userId,
                cancellationToken);
    }
    public async Task<Guid?> GetTicketCustomerUserIdAsync(
    Guid organizationId,
    Guid ticketId,
    CancellationToken cancellationToken = default)
    {
        return await Tickets
            .Where(ticket =>
                ticket.Id == ticketId &&
                ticket.OrganizationId == organizationId)
            .Select(ticket =>
                Customers
                    .Where(customer =>
                        customer.Id == ticket.CustomerId &&
                        customer.OrganizationId == organizationId &&
                        customer.IsActive)
                    .Select(customer => customer.UserId)
                    .FirstOrDefault())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DashboardTicketStatistics>
    GetDashboardTicketStatisticsAsync(
        Guid organizationId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = Tickets
            .AsNoTracking()
            .Where(ticket =>
                ticket.OrganizationId == organizationId);

        if (from.HasValue)
        {
            query = query.Where(ticket =>
                ticket.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(ticket =>
                ticket.CreatedAt <= to.Value);
        }

        var result = await query
            .GroupBy(_ => 1)
.Select(group => new DashboardTicketStatistics(
    group.Count(),

    group.Count(ticket =>
        ticket.Status == TicketStatus.New),

    group.Count(ticket =>
        ticket.Status == TicketStatus.Open),

    group.Count(ticket =>
        ticket.Status == TicketStatus.InProgress),

    group.Count(ticket =>
        ticket.Status == TicketStatus.WaitingForCustomer),

    group.Count(ticket =>
        ticket.Status == TicketStatus.Resolved),

    group.Count(ticket =>
        ticket.Status == TicketStatus.Closed),

    group.Count(ticket =>
        ticket.Priority == TicketPriority.Low),

    group.Count(ticket =>
        ticket.Priority == TicketPriority.Medium),

    group.Count(ticket =>
        ticket.Priority == TicketPriority.High),

    group.Count(ticket =>
        ticket.Priority == TicketPriority.Critical),

    group.Count(ticket =>
        ticket.ResolvedAt.HasValue),

    group.Count(ticket =>
        ticket.ClosedAt.HasValue),

    group
        .Where(ticket => ticket.ResolvedAt.HasValue)
        .Average(ticket =>
            (double?)EF.Functions.DateDiffSecond(
                ticket.CreatedAt,
                ticket.ResolvedAt!.Value)),

    group
        .Where(ticket => ticket.ClosedAt.HasValue)
        .Average(ticket =>
            (double?)EF.Functions.DateDiffSecond(
                ticket.CreatedAt,
                ticket.ClosedAt!.Value))
)).FirstOrDefaultAsync(cancellationToken);

        return result
            ?? new DashboardTicketStatistics(
                TotalTickets: 0,
                NewTickets: 0,
                OpenTickets: 0,
                InProgressTickets: 0,
                WaitingForCustomerTickets: 0,
                ResolvedTickets: 0,
                ClosedTickets: 0,
                LowPriorityTickets: 0,
                MediumPriorityTickets: 0,
                HighPriorityTickets: 0,
                CriticalPriorityTickets: 0,
                ResolvedCount: 0,
                ClosedCount: 0,
                AverageResolutionSeconds: null,
                AverageClosureSeconds: null);
    }
    public async Task<OrganizationReportingCounts>
    GetOrganizationReportingCountsAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var totalCustomers = await Customers
            .AsNoTracking()
            .CountAsync(
                customer =>
                    customer.OrganizationId == organizationId &&
                    customer.IsActive,
                cancellationToken);

        var totalCategories = await Categories
            .AsNoTracking()
            .CountAsync(
                category =>
                    category.OrganizationId == organizationId &&
                    category.IsActive,
                cancellationToken);

        var totalTeams = await Teams
            .AsNoTracking()
            .CountAsync(
                team =>
                    team.OrganizationId == organizationId,
                cancellationToken);

        var totalAgents = await OrganizationMembers
            .AsNoTracking()
            .CountAsync(
                member =>
                    member.OrganizationId == organizationId &&
                    member.Role == OrganizationRole.Agent,
                cancellationToken);

        return new OrganizationReportingCounts(
            TotalCustomers: totalCustomers,
            TotalCategories: totalCategories,
            TotalTeams: totalTeams,
            TotalAgents: totalAgents);
    }
    public async Task<TicketStatisticsQueryResult>
    GetTicketStatisticsAsync(
        Guid organizationId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var ticketsQuery = Tickets
            .AsNoTracking()
            .Where(ticket =>
                ticket.OrganizationId == organizationId);

        if (from.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(ticket =>
                ticket.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(ticket =>
                ticket.CreatedAt <= to.Value);
        }

        var statusDistribution = await ticketsQuery
            .GroupBy(ticket => ticket.Status)
            .Select(group => new TicketStatusStatisticResult(
                group.Key,
                group.Count()))
            .ToListAsync(cancellationToken);

        var priorityDistribution = await ticketsQuery
            .GroupBy(ticket => ticket.Priority)
            .Select(group => new TicketPriorityStatisticResult(
                group.Key,
                group.Count()))
            .ToListAsync(cancellationToken);

        var categoryDistribution = await ticketsQuery
            .Join(
                Categories.AsNoTracking(),
                ticket => ticket.CategoryId,
                category => category.Id,
                (ticket, category) => new
                {
                    Ticket = ticket,
                    Category = category
                })
            .Where(x =>
                x.Category.OrganizationId == organizationId)
            .GroupBy(x => new
            {
                x.Category.Id,
                x.Category.Name
            })
            .Select(group => new CategoryTicketStatisticResult(
                group.Key.Id,
                group.Key.Name,
                group.Count()))
            .OrderByDescending(result => result.TicketCount)
            .ThenBy(result => result.CategoryName)
            .ToListAsync(cancellationToken);

        var resolvedCount = await ticketsQuery
            .CountAsync(
                ticket => ticket.ResolvedAt.HasValue,
                cancellationToken);

        var closedCount = await ticketsQuery
            .CountAsync(
                ticket => ticket.ClosedAt.HasValue,
                cancellationToken);

        var averageResolutionSeconds = await ticketsQuery
            .Where(ticket => ticket.ResolvedAt.HasValue)
            .Select(ticket =>
                (double?)EF.Functions.DateDiffSecond(
                    ticket.CreatedAt,
                    ticket.ResolvedAt!.Value))
            .AverageAsync(cancellationToken);

        var averageClosureSeconds = await ticketsQuery
            .Where(ticket => ticket.ClosedAt.HasValue)
            .Select(ticket =>
                (double?)EF.Functions.DateDiffSecond(
                    ticket.CreatedAt,
                    ticket.ClosedAt!.Value))
            .AverageAsync(cancellationToken);

        return new TicketStatisticsQueryResult(
            statusDistribution,
            priorityDistribution,
            categoryDistribution,
            resolvedCount,
            closedCount,
            averageResolutionSeconds,
            averageClosureSeconds);
    }
    public async Task<IReadOnlyList<TeamStatisticsQueryResult>>
    GetTeamStatisticsAsync(
        Guid organizationId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var teamsQuery = Teams
            .AsNoTracking()
            .Where(team =>
                team.OrganizationId == organizationId);

        var ticketsQuery = Tickets
            .AsNoTracking()
            .Where(ticket =>
                ticket.OrganizationId == organizationId);

        if (from.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(ticket =>
                ticket.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(ticket =>
                ticket.CreatedAt <= to.Value);
        }

        var result = await teamsQuery
            .Select(team => new TeamStatisticsQueryResult(
                team.Id,
                team.Name,

                TeamMembers.Count(member =>
                    member.TeamId == team.Id),

                ticketsQuery.Count(ticket =>
                    ticket.TeamId == team.Id),

                ticketsQuery.Count(ticket =>
                    ticket.TeamId == team.Id &&
                    (ticket.Status == TicketStatus.New ||
                     ticket.Status == TicketStatus.Open ||
                     ticket.Status == TicketStatus.InProgress ||
                     ticket.Status == TicketStatus.WaitingForCustomer)),

                ticketsQuery.Count(ticket =>
                    ticket.TeamId == team.Id &&
                    ticket.Status == TicketStatus.Resolved),

                ticketsQuery.Count(ticket =>
                    ticket.TeamId == team.Id &&
                    ticket.Status == TicketStatus.Closed)
            ))
            .OrderByDescending(result => result.TotalTickets)
            .ThenBy(result => result.TeamName)
            .ToListAsync(cancellationToken);

        return result;
    }
    public async Task<IReadOnlyList<AgentStatisticsQueryResult>>
    GetAgentStatisticsAsync(
        Guid organizationId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var ticketsQuery = Tickets
            .AsNoTracking()
            .Where(ticket =>
                ticket.OrganizationId == organizationId);

        if (from.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(ticket =>
                ticket.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(ticket =>
                ticket.CreatedAt <= to.Value);
        }

        var result = await OrganizationMembers
            .AsNoTracking()
            .Where(member =>
                member.OrganizationId == organizationId &&
                member.Role == OrganizationRole.Agent)
            .Join(
                Users.AsNoTracking(),
                member => member.UserId,
                user => user.Id,
                (member, user) => new
                {
                    AgentId = member.UserId,
                    AgentUserName = user.UserName
                })
            .Select(agent => new AgentStatisticsQueryResult(
                agent.AgentId,
                agent.AgentUserName ?? string.Empty,

                ticketsQuery.Count(ticket =>
                    ticket.AssignedAgentId == agent.AgentId),

                ticketsQuery.Count(ticket =>
                    ticket.AssignedAgentId == agent.AgentId &&
                    (ticket.Status == TicketStatus.New ||
                     ticket.Status == TicketStatus.Open ||
                     ticket.Status == TicketStatus.InProgress ||
                     ticket.Status == TicketStatus.WaitingForCustomer)),

                ticketsQuery.Count(ticket =>
                    ticket.AssignedAgentId == agent.AgentId &&
                    ticket.Status == TicketStatus.Resolved),

                ticketsQuery.Count(ticket =>
                    ticket.AssignedAgentId == agent.AgentId &&
                    ticket.Status == TicketStatus.Closed)
            ))
            .OrderByDescending(result => result.AssignedTickets)
            .ThenBy(result => result.AgentUserName)
            .ToListAsync(cancellationToken);

        return result;
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ServiCoreDbContext).Assembly);
    }
}