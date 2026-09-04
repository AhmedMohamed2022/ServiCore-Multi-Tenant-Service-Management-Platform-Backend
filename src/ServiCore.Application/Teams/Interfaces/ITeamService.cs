using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Application.Common.Results;
using ServiCore.Application.Teams.DTOs;

namespace ServiCore.Application.Teams.Interfaces;

public interface ITeamService
{
    Task<Result<TeamDto>> CreateAsync(string name, string? description, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TeamDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TeamDto>> GetByIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<Result<TeamDto>> UpdateAsync(
    Guid teamId,
    string name,
    string? description,
    CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(
    Guid teamId,
    CancellationToken cancellationToken = default);
}