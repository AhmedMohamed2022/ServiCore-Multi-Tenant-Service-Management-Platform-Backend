using ServiCore.Application.Common.Results;
using ServiCore.Application.Teams.DTOs;

namespace ServiCore.Application.Teams.Interfaces;

public interface ITeamMembershipService
{
    Task<Result> AddMemberAsync(
        Guid teamId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TeamMemberDto>>> GetMembersAsync(
        Guid teamId,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveMemberAsync(
        Guid teamId,
        Guid userId,
        CancellationToken cancellationToken = default);
}