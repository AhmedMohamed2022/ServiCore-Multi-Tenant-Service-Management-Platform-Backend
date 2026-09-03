using Microsoft.Extensions.DependencyInjection;
using ServiCore.Application.Authentication.Interfaces;
using ServiCore.Application.Authentication.Services;
using ServiCore.Application.Organizations.Interfaces;
using ServiCore.Application.Organizations.Services;
using ServiCore.Application.Teams.Interfaces;
using ServiCore.Application.Teams.Services;

namespace ServiCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IAuthenticationService,AuthenticationService>();

        return services;
    }
}