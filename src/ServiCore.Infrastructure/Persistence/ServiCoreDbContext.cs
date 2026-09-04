using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Domain.Entities;
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

    public DbSet<OrganizationMember> OrganizationMembers
        => Set<OrganizationMember>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Category> Categories => Set<Category>();

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

    public async Task<bool> OrganizationMemberExistsAsync(Guid organizationId,Guid userId,CancellationToken cancellationToken = default)
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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ServiCoreDbContext).Assembly);
    }
}