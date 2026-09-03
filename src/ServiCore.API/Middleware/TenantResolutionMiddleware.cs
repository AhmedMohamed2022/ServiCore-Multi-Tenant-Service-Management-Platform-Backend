using ServiCore.Application.Common.Interfaces;
using ServiCore.Infrastructure.Common;
using System.Security.Claims;

namespace ServiCore.API.Middleware;

public class TenantResolutionMiddleware
{
    private const string OrganizationHeader =
        "X-Organization-Id";

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICurrentUser currentUser,
        ITenantResolver tenantResolver,
        ITenantContext tenantContext)
    {
        if (!currentUser.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        var organizationHeader =
            context.Request.Headers[
                OrganizationHeader].FirstOrDefault();

        if (!Guid.TryParse(
                organizationHeader,
                out var organizationId))
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    error =
                        "A valid X-Organization-Id header is required."
                });

            return;
        }

        var userId = currentUser.UserId;

        if (!userId.HasValue)
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            return;
        }

        var result =
            await tenantResolver.ResolveAsync(
                userId.Value,
                organizationId,
                context.RequestAborted);

        if (!result.IsSuccess)
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            await context.Response.WriteAsJsonAsync(
                new
                {
                    error = result.Error
                });

            return;
        }

        var resolvedOrganizationId = result.Value;

        ((TenantContext)tenantContext)
            .SetOrganization(resolvedOrganizationId);
        await _next(context);
    }
}