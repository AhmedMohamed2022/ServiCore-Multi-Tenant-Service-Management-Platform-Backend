using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ServiCore.Application.Common.Interfaces;

namespace ServiCore.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private const string OrganizationHeader = "X-Organization-Id";

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

        var organizationHeader =
            httpContext?
                .Request
                .Headers[OrganizationHeader]
                .FirstOrDefault();

        if (!Guid.TryParse(
                organizationHeader,
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

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}