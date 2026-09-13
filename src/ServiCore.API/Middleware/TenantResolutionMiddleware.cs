using ServiCore.Application.Common.Interfaces;
using ServiCore.Infrastructure.Common;

namespace ServiCore.API.Middleware;

public class TenantResolutionMiddleware
{
    private const string OrganizationHeader = "X-Organization-Id";

    // Requests that are meaningful for an authenticated user
    // *before* a tenant has been resolved — must never be blocked
    // by the tenant header check.
    private static readonly string[] TenantExemptPaths =
    {
        "/api/auth/me",
        "/api/organizations/mine",
        "/hubs", // SignalR hubs resolve tenant themselves (see
                 // NotificationHub.OnConnectedAsync). A WebSocket upgrade
                 // can't carry the X-Organization-Id header, so the generic
                 // header check here would block every hub connection
                 // outright, regardless of validity.
    };

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
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

        if (IsTenantExempt(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var organizationHeader =
            context.Request.Headers[OrganizationHeader].FirstOrDefault();

        if (!Guid.TryParse(organizationHeader, out var organizationId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "A valid X-Organization-Id header is required."
            });
            return;
        }

        var userId = currentUser.UserId;
        if (!userId.HasValue)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var result = await tenantResolver.ResolveAsync(
            userId.Value, organizationId, context.RequestAborted);

        if (!result.IsSuccess)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = result.Error });
            return;
        }

        ((TenantContext)tenantContext).SetOrganization(result.Value);
        await _next(context);
    }

    private static bool IsTenantExempt(PathString path)
    {
        foreach (var exempt in TenantExemptPaths)
        {
            if (path.StartsWithSegments(exempt, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}