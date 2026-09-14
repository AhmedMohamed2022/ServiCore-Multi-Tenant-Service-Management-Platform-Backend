using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;
using ServiCore.Infrastructure.Persistence;
using ServiCore.IntegrationTests.Common;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public sealed class NotificationApiTests : IntegrationTestBase
{
    [Fact]
    public async Task GetNotifications_ShouldReturnOnlyCurrentUsersCurrentTenantNotifications()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();

        await factory.ResetDatabaseAsync();

        using var ownerClient = factory.CreateClient();
        var organization = await RegisterAndLoginAsync(
            ownerClient,
            "owner@test.com",
            "Organization A");
        SelectOrganization(ownerClient, organization.OrganizationId);

        using var secondClient = factory.CreateClient();
        var secondOrganization = await RegisterAndLoginAsync(
            secondClient,
            "second@test.com",
            "Organization B");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ServiCoreDbContext>();

            db.Notifications.AddRange(
                new Notification(
                    organization.UserId,
                    organization.OrganizationId,
                    NotificationType.TicketAssigned,
                    "Own notification",
                    "Visible to owner."),
                new Notification(
                    secondOrganization.UserId,
                    organization.OrganizationId,
                    NotificationType.TicketAssigned,
                    "Other user notification",
                    "Must not be visible."),
                new Notification(
                    organization.UserId,
                    secondOrganization.OrganizationId,
                    NotificationType.TicketAssigned,
                    "Other tenant notification",
                    "Must not be visible."));

            await db.SaveChangesAsync();
        }

        var response = await ownerClient.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<List<NotificationResponse>>();

        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("Own notification", body[0].Title);
    }

    private sealed record NotificationResponse(
        Guid Id,
        int Type,
        string Title,
        string Message,
        Guid? RelatedEntityId,
        DateTime CreatedAt,
        DateTime? ReadAt,
        bool IsRead);
}
