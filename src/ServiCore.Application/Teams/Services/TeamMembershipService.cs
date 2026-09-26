using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;
using ServiCore.Application.Teams.DTOs;
using ServiCore.Application.Teams.Interfaces;
using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;

namespace ServiCore.Application.Teams.Services;

public class TeamMembershipService : ITeamMembershipService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public TeamMembershipService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
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

        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure(
                "An authenticated user is required.");
        }

        if (userId == Guid.Empty)
        {
            return Result.Failure(
                "User ID is required.");
        }

        var organizationId =
            _tenantContext.OrganizationId.Value;

        var currentUserId =
            _currentUser.UserId.Value;

        var team = await _dbContext.GetTeamAsync(
            organizationId,
            teamId,
            cancellationToken);

        if (team is null || !team.IsActive)
        {
            return Result.Failure(
                "Team not found.");
        }

        var currentUserRole =
            await _dbContext.GetOrganizationRoleAsync(
                organizationId,
                currentUserId,
                cancellationToken);

        if (!currentUserRole.HasValue)
        {
            return Result.Failure(
                "The current user is not a member of this organization.");
        }

        // Owner can manage membership for any team.
        if (currentUserRole.Value == OrganizationRole.Owner)
        {
            // Owner may continue below.
        }
        // Manager may only manage agents in teams
        // that the manager belongs to.
        else if (currentUserRole.Value == OrganizationRole.Manager)
        {
            var targetRole =
            await _dbContext.GetOrganizationRoleAsync(
                  organizationId,
                  userId,
                  cancellationToken);

            if (targetRole != OrganizationRole.Manager &&
                targetRole != OrganizationRole.Agent)
            {
                return Result.Failure(
                    "Only managers and agents can be added to teams.");
            }
                var managerBelongsToTeam =
                    await _dbContext.TeamMemberExistsAsync(
                        teamId,
                        currentUserId,
                        cancellationToken);

                if (!managerBelongsToTeam)
                {
                    return Result.Failure(
                        "You can only manage members of your own teams.");
                }

                if (targetRole != OrganizationRole.Agent)
                {
                    return Result.Failure(
                        "Managers can only add agents to their teams.");
                }
            
        }
        else
        {
            return Result.Failure(
                "You are not authorized to manage team membership.");
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

        var targetRoleForOwner =
            await _dbContext.GetOrganizationRoleAsync(
                organizationId,
                userId,
                cancellationToken);

        if (targetRoleForOwner != OrganizationRole.Manager &&
            targetRoleForOwner != OrganizationRole.Agent)
        {
            return Result.Failure(
                "Only managers and agents can be added to teams.");
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

    public async Task<Result<IReadOnlyList<TeamMemberDto>>> GetMembersAsync(
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

        if (!_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<TeamMemberDto>>.Failure(
                "An authenticated user is required.");
        }

        var currentUserId = _currentUser.UserId.Value;

        var team = await _dbContext.GetTeamAsync(
            organizationId,
            teamId,
            cancellationToken);

        if (team is null || !team.IsActive)
        {
            return Result<IReadOnlyList<TeamMemberDto>>.Failure(
                "Team not found.");
        }

        var currentUserRole =
            await _dbContext.GetOrganizationRoleAsync(
                organizationId,
                currentUserId,
                cancellationToken);

        if (currentUserRole == OrganizationRole.Manager)
        {
            var managerBelongsToTeam =
                await _dbContext.TeamMemberExistsAsync(
                    teamId,
                    currentUserId,
                    cancellationToken);

            if (!managerBelongsToTeam)
            {
                return Result<IReadOnlyList<TeamMemberDto>>.Failure(
                    "You can only view members of your own teams.");
            }
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

        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure(
                "An authenticated user is required.");
        }

        if (userId == Guid.Empty)
        {
            return Result.Failure(
                "User ID is required.");
        }

        var organizationId =
            _tenantContext.OrganizationId.Value;

        var currentUserId =
            _currentUser.UserId.Value;

        var team = await _dbContext.GetTeamAsync(
            organizationId,
            teamId,
            cancellationToken);

        if (team is null || !team.IsActive)
        {
            return Result.Failure(
                "Team not found.");
        }

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

        var currentUserRole =
            await _dbContext.GetOrganizationRoleAsync(
                organizationId,
                currentUserId,
                cancellationToken);

        if (!currentUserRole.HasValue)
        {
            return Result.Failure(
                "The current user is not a member of this organization.");
        }

        if (currentUserRole.Value == OrganizationRole.Owner)
        {
            // Owner can remove any team member.
        }
        else if (currentUserRole.Value == OrganizationRole.Manager)
        {
            var managerBelongsToTeam =
                await _dbContext.TeamMemberExistsAsync(
                    teamId,
                    currentUserId,
                    cancellationToken);

            if (!managerBelongsToTeam)
            {
                return Result.Failure(
                    "You can only manage members of your own teams.");
            }

            var targetRole =
                await _dbContext.GetOrganizationRoleAsync(
                    organizationId,
                    userId,
                    cancellationToken);

            if (targetRole != OrganizationRole.Agent)
            {
                return Result.Failure(
                    "Managers can only remove agents from their teams.");
            }
        }
        else
        {
            return Result.Failure(
                "You are not authorized to manage team membership.");
        }

        _dbContext.RemoveTeamMember(member);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}