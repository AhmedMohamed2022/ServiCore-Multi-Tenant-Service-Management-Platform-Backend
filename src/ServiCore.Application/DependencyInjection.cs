using Microsoft.Extensions.DependencyInjection;
using ServiCore.Application.Authentication.Interfaces;
using ServiCore.Application.Authentication.Services;
using ServiCore.Application.Categories.Interfaces;
using ServiCore.Application.Categories.Services;
using ServiCore.Application.Customers.Interfaces;
using ServiCore.Application.Customers.Services;
using ServiCore.Application.Organizations.Interfaces;
using ServiCore.Application.Organizations.Services;
using ServiCore.Application.Teams.Interfaces;
using ServiCore.Application.Teams.Services;
using ServiCore.Application.Tickets.Interfaces;
using ServiCore.Application.Tickets.Services;

namespace ServiCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITeamMembershipService, TeamMembershipService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITicketService, TicketService>();
        return services;
    }
}