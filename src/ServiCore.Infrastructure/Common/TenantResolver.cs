using Microsoft.EntityFrameworkCore;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;
using ServiCore.Infrastructure.Persistence;

namespace ServiCore.Infrastructure.Common;

public class TenantResolver : ITenantResolver
{
    private readonly ServiCoreDbContext _dbContext;

    public TenantResolver(
        ServiCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> ResolveAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var isOrganizationMember =
            await _dbContext.OrganizationMembers
                .AnyAsync(
                    x =>
                        x.UserId == userId &&
                        x.OrganizationId == organizationId,
                    cancellationToken);

        if (isOrganizationMember)
        {
            return Result<Guid>.Success(organizationId);
        }

        var isCustomer =
            await _dbContext.Customers
                .AnyAsync(
                    x =>
                        x.UserId == userId &&
                        x.OrganizationId == organizationId &&
                        x.IsActive,
                    cancellationToken);

        if (isCustomer)
        {
            return Result<Guid>.Success(organizationId);
        }

        return Result<Guid>.Failure(
            "The user does not have access to this organization.");
    }
}