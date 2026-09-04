using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Domain.Enums;
using ServiCore.Infrastructure.Persistence;

namespace ServiCore.Infrastructure.Authentication.Authorization;

public class OrganizationRoleAuthorizationHandler
    : AuthorizationHandler<OrganizationRoleRequirement>
{
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly ServiCoreDbContext _dbContext;

    public OrganizationRoleAuthorizationHandler(
        ICurrentUser currentUser,
        ITenantContext tenantContext,
        ServiCoreDbContext dbContext)
    {
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganizationRoleRequirement requirement)
    {
        if (!_currentUser.UserId.HasValue)
            return;

        if (!_tenantContext.OrganizationId.HasValue)
            return;

        var userId =
            _currentUser.UserId.Value;

        var organizationId =
            _tenantContext.OrganizationId.Value;

        var role =
            await _dbContext.OrganizationMembers
                .Where(x =>
                    x.UserId == userId &&
                    x.OrganizationId == organizationId)
                .Select(x => (OrganizationRole?)x.Role)
                .SingleOrDefaultAsync();

        if (role.HasValue &&
            requirement.AllowedRoles.Contains(role.Value))
        {
            context.Succeed(requirement);
        }
    }
}