using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ServiCore.IntegrationTests.Common;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public sealed class TeamsApiTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateTeam_WithAuthenticatedOwnerAndValidTenant_ShouldSucceedAndPersist()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var organization = await RegisterAndLoginAsync(
            client,
            "owner@test.com",
            "Organization A");

        SelectOrganization(client, organization.OrganizationId);

        var response = await client.PostAsJsonAsync(
            "/api/teams",
            new
            {
                name = "Support Team",
                description = "Handles customer support."
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var team = await response.Content
            .ReadFromJsonAsync<TeamTestResponse>();

        Assert.NotNull(team);
        Assert.Equal(organization.OrganizationId, team.OrganizationId);
        Assert.Equal("Support Team", team.Name);
        Assert.Equal("Handles customer support.", team.Description);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<ServiCore.Infrastructure.Persistence.ServiCoreDbContext>();

        var persisted = await db.Teams.FindAsync(team.Id);

        Assert.NotNull(persisted);
        Assert.Equal(organization.OrganizationId, persisted.OrganizationId);
    }
}
