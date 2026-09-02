using Microsoft.Extensions.DependencyInjection;
using ServiCore.Application.Organizations.Interfaces;
using ServiCore.Application.Organizations.Services;

namespace ServiCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IOrganizationService, OrganizationService>();

        return services;
    }
}