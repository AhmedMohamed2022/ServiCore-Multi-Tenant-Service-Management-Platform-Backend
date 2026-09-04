using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;
using ServiCore.Application.Teams.DTOs;
using ServiCore.Application.Teams.Interfaces;
using ServiCore.Domain.Entities;

namespace ServiCore.Application.Teams.Services;

public class TeamService : ITeamService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public TeamService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<TeamDto>> CreateAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<TeamDto>.Failure(
                "An organization context is required.");
        }

        var organizationId = _tenantContext.OrganizationId.Value;

        var team = new Team(
            organizationId,
            name,
            description);

        _dbContext.AddTeam(team);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new TeamDto(
            team.Id,
            team.OrganizationId,
            team.Name,
            team.Description,
            team.CreatedAt);

        return Result<TeamDto>.Success(dto);
    }
    public async Task<Result<IReadOnlyList<TeamDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<IReadOnlyList<TeamDto>>.Failure(
                "An organization context is required.");
        }

        var organizationId =
            _tenantContext.OrganizationId.Value;

        var teams = await _dbContext.GetTeamsAsync(organizationId, cancellationToken);

        var result = teams
            .Select(x => new TeamDto(
                x.Id,
                x.OrganizationId,
                x.Name,
                x.Description,
                x.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<TeamDto>>.Success(result);
    }
    public async Task<Result<TeamDto>> GetByIdAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<TeamDto>.Failure(
                "An organization context is required.");
        }

        var organizationId =
            _tenantContext.OrganizationId.Value;

        var team = await _dbContext.GetTeamAsync(organizationId, teamId, cancellationToken);

        if (team is null || !team.IsActive)
        {
            return Result<TeamDto>.Failure(
                "Team not found.");
        }

        var dto = new TeamDto(
            team.Id,
            team.OrganizationId,
            team.Name,
            team.Description,
            team.CreatedAt);

        return Result<TeamDto>.Success(dto);
    }
    public async Task<Result<TeamDto>> UpdateAsync(Guid teamId, string name, string? description, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result<TeamDto>.Failure(
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
            return Result<TeamDto>.Failure(
                "Team not found.");
        }

        try
        {
            team.Update(name, description);
        }
        catch (ArgumentException ex)
        {
            return Result<TeamDto>.Failure(
                ex.Message);
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var dto = new TeamDto(
            team.Id,
            team.OrganizationId,
            team.Name,
            team.Description,
            team.CreatedAt);

        return Result<TeamDto>.Success(dto);
    }
    public async Task<Result> DeactivateAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.OrganizationId.HasValue)
        {
            return Result.Failure(
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
            return Result.Failure(
                "Team not found.");
        }

        team.Deactivate();

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}