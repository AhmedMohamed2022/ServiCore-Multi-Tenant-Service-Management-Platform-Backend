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
    Task<IReadOnlyList<Team>> GetTeamsAsync(
    Guid organizationId,
    CancellationToken cancellationToken = default);

    Task<Team?> GetTeamAsync(
        Guid organizationId,
        Guid teamId,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<IApplicationTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);
}