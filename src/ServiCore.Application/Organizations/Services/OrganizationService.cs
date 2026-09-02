using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;
using ServiCore.Application.Organizations.DTOs;
using ServiCore.Application.Organizations.Interfaces;
using ServiCore.Domain.Entities;

namespace ServiCore.Application.Organizations.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IApplicationDbContext _dbContext;

    public OrganizationService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<OrganizationDto>> CreateAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var organization = new Organization(name);

        _dbContext.AddOrganization(organization);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.CreatedAt);

        return Result<OrganizationDto>.Success(result);
    }
}
