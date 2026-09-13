using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ServiCore.Application.Common.Interfaces;

namespace ServiCore.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private const string OrganizationHeader = "X-Organization-Id";
    private const string OrganizationQueryParam = "organizationId";

    private readonly ITenantResolver _tenantResolver;

    public NotificationHub(
        ITenantResolver tenantResolver)
    {
        _tenantResolver = tenantResolver;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            Context.Abort();
            return;
        }

        var httpContext = Context.GetHttpContext();

        // WebSocket/SSE transports can't carry custom headers from the
        // browser, so the org id travels as a query string parameter —
        // the header is kept only as a fallback for HTTP-based transports.
        var organizationValue =
            httpContext?.Request.Query[OrganizationQueryParam].FirstOrDefault()
            ?? httpContext?.Request.Headers[OrganizationHeader].FirstOrDefault();

        if (!Guid.TryParse(
                organizationValue,
                out var organizationId))
        {
            Context.Abort();
            return;
        }

        var result = await _tenantResolver.ResolveAsync(
            parsedUserId,
            organizationId,
            Context.ConnectionAborted);

        if (!result.IsSuccess)
        {
            Context.Abort();
            return;
        }

        var groupName = GetUserGroupName(
            organizationId,
            parsedUserId);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            groupName,
            Context.ConnectionAborted);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public static string GetUserGroupName(
        Guid organizationId,
        Guid userId)
    {
        return $"organization:{organizationId}:user:{userId}";
    }
}