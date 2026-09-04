using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Domain.Entities;

namespace ServiCore.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    void AddOrganization(Organization organization);

    void AddOrganizationMember(OrganizationMember member);

    void AddTeam(Team team);
    Task<IReadOnlyList<Team>> GetTeamsAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<Team?> GetTeamAsync(Guid organizationId, Guid teamId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IApplicationTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    void AddTeamMember(TeamMember member);

    Task<bool> TeamMemberExistsAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeamMember>> GetTeamMembersAsync(Guid organizationId, Guid teamId, CancellationToken cancellationToken = default);

    Task<TeamMember?> GetTeamMemberAsync(Guid organizationId, Guid teamId, Guid userId, CancellationToken cancellationToken = default);

    void RemoveTeamMember(TeamMember member);
    Task<bool> OrganizationMemberExistsAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default);
    void AddCustomer(Customer customer);

    Task<IReadOnlyList<Customer>> GetCustomersAsync(Guid organizationId, CancellationToken cancellationToken = default);

    Task<Customer?> GetCustomerAsync(Guid organizationId, Guid customerId, bool activeOnly = true, CancellationToken cancellationToken = default);

    Task<bool> CustomerEmailExistsAsync(Guid organizationId, string email, Guid? excludingCustomerId = null, CancellationToken cancellationToken = default);
}