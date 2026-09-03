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
        var exists = await _dbContext.OrganizationMembers
            .AnyAsync(
                x =>
                    x.UserId == userId &&
                    x.OrganizationId == organizationId,
                cancellationToken);

        if (!exists)
        {
            return Result<Guid>.Failure(
                "The user is not a member of this organization.");
        }

        return Result<Guid>.Success(
            organizationId);
    }
}