using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Domain.Enums;
using ServiCore.Infrastructure.Authentication;
using ServiCore.Infrastructure.Authentication.Authorization;
using ServiCore.Infrastructure.Common;
using ServiCore.Infrastructure.Identity;
using ServiCore.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ServiCoreDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
        }).AddSignInManager()
        .AddEntityFrameworkStores<ServiCoreDbContext>();

        var jwtSection = configuration.GetSection(
    JwtOptions.SectionName);

        services.Configure<JwtOptions>(options =>
        {
            options.Issuer =
                jwtSection["Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is not configured.");

            options.Audience =
                jwtSection["Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is not configured.");

            options.SecretKey =
                configuration["JWT_SECRET_KEY"]
                ?? throw new InvalidOperationException(
                    "JWT secret key is not configured.");
        });

        services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions =
            jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration is missing.");

        var secretKey =
            configuration["JWT_SECRET_KEY"]
            ?? throw new InvalidOperationException(
                "JWT secret key is not configured.");

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "CanManageTeams",
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.AddRequirements(
                        new OrganizationRoleRequirement(
                            OrganizationRole.Owner,
                            OrganizationRole.Manager));
                });
            options.AddPolicy(
                "CanManageCategories",
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.AddRequirements(
                        new OrganizationRoleRequirement(
                            OrganizationRole.Owner,
                            OrganizationRole.Manager));
                });
        });

        services.AddScoped<IAuthorizationHandler, OrganizationRoleAuthorizationHandler>();

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IApplicationDbContext>(
    provider => provider.GetRequiredService<ServiCoreDbContext>());

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ITenantResolver, TenantResolver>();

        return services;
    }
}
