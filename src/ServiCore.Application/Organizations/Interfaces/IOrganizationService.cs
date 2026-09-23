using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Application.Common.Results;
using ServiCore.Application.Organizations.DTOs;

namespace ServiCore.Application.Organizations.Interfaces;

public interface IOrganizationService
{
    Task<Result<OrganizationDto>> CreateAsync(
        string name,
        CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<OrganizationDto>>> GetMineAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every member of the active (tenant-resolved) organization, with their
    /// role. Backs the Owner's "add a Manager to a team" picker — see
    /// OrganizationMemberDto for why this exists at all.
    /// </summary>
    Task<Result<IReadOnlyList<OrganizationMemberDto>>> GetMembersAsync(CancellationToken cancellationToken = default);
}