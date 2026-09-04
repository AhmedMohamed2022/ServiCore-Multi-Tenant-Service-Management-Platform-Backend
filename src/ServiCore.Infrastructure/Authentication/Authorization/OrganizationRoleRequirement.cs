using Microsoft.AspNetCore.Authorization;
using ServiCore.Domain.Enums;

namespace ServiCore.Infrastructure.Authentication.Authorization;

public class OrganizationRoleRequirement
    : IAuthorizationRequirement
{
    public IReadOnlyCollection<OrganizationRole> AllowedRoles { get; }

    public OrganizationRoleRequirement(
        params OrganizationRole[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}