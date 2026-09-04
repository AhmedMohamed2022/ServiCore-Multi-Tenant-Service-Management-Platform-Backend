using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;
using ServiCore.Application.Teams.DTOs;
using ServiCore.Application.Teams.Interfaces;
using ServiCore.Domain.Entities;

namespace ServiCore.Application.Teams.Services;

public class TeamMembershipService : ITeamMembershipService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public TeamMembershipService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result> AddMemberAsync(
        Guid teamId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result.Failure(
                "An organization context is required.");
        }

        if (userId == Guid.Empty)
        {
            return Result.Failure(
                "User ID is required.");
        }

        var organizationId =
            _tenantContext.OrganizationId.Value;

        var team = await _dbContext.GetTeamAsync(
            organizationId,
            teamId,
            cancellationToken);

        if (team is null || !team.IsActive)
        {
            return Result.Failure(
                "Team not found.");
        }

        var organizationMemberExists =
            await _dbContext.OrganizationMemberExistsAsync(
                organizationId,
                userId,
                cancellationToken);

        if (!organizationMemberExists)
        {
            return Result.Failure(
                "The user is not a member of this organization.");
        }

        var alreadyMember =
            await _dbContext.TeamMemberExistsAsync(
                teamId,
                userId,
                cancellationToken);

        if (alreadyMember)
        {
            return Result.Failure(
                "The user is already a member of this team.");
        }

        var teamMember = new TeamMember(
            teamId,
            userId);

        _dbContext.AddTeamMember(teamMember);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<TeamMemberDto>>>
        GetMembersAsync(
            Guid teamId,
            CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<IReadOnlyList<TeamMemberDto>>.Failure(
                "An organization context is required.");
        }

        var organizationId =
            _tenantContext.OrganizationId.Value;

        var team = await _dbContext.GetTeamAsync(
            organizationId,
            teamId,
            cancellationToken);

        if (team is null || !team.IsActive)
        {
            return Result<IReadOnlyList<TeamMemberDto>>.Failure(
                "Team not found.");
        }

        var members =
            await _dbContext.GetTeamMembersAsync(
                organizationId,
                teamId,
                cancellationToken);

        var result = members
            .Select(x => new TeamMemberDto(
                x.TeamId,
                x.UserId,
                x.JoinedAt))
            .ToList();

        return Result<IReadOnlyList<TeamMemberDto>>
            .Success(result);
    }

    public async Task<Result> RemoveMemberAsync(
        Guid teamId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result.Failure(
                "An organization context is required.");
        }

        var organizationId =
            _tenantContext.OrganizationId.Value;

        var member =
            await _dbContext.GetTeamMemberAsync(
                organizationId,
                teamId,
                userId,
                cancellationToken);

        if (member is null)
        {
            return Result.Failure(
                "Team member not found.");
        }

        _dbContext.RemoveTeamMember(member);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}