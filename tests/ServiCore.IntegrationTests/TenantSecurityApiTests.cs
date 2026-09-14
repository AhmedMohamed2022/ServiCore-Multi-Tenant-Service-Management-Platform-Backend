using System.Net;
using System.Net.Http.Json;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public sealed class TenantSecurityApiTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateTeam_WithoutAuthentication_ShouldReturn401()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/teams",
            new { name = "Unauthorized Team", description = "No access." });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTeam_WithoutTenantHeader_ShouldReturn400()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        await RegisterAndLoginAsync(client, "owner@test.com", "Organization A");

        var response = await client.PostAsJsonAsync(
            "/api/teams",
            new { name = "Support Team", description = "Handles support." });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTeam_WithAnotherOrganizationsTenant_ShouldReturn403()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();

        using var clientA = factory.CreateClient();
        var organizationA = await RegisterAndLoginAsync(
            clientA, "ownerA@test.com", "Organization A");

        using var clientB = factory.CreateClient();
        var organizationB = await RegisterAndLoginAsync(
            clientB, "ownerB@test.com", "Organization B");

        SelectOrganization(clientA, organizationB.OrganizationId);

        var response = await clientA.PostAsJsonAsync(
            "/api/teams",
            new { name = "Cross Tenant Team", description = "Must fail." });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        SelectOrganization(clientA, organizationA.OrganizationId);

        var validResponse = await clientA.PostAsJsonAsync(
            "/api/teams",
            new { name = "Organization A Team", description = "Valid." });

        Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);
    }

    [Fact]
    public async Task GetTeams_ShouldReturnOnlyCurrentTenantTeams()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();

        using var clientA = factory.CreateClient();
        var organizationA = await RegisterAndLoginAsync(
            clientA, "ownerA@test.com", "Organization A");
        SelectOrganization(clientA, organizationA.OrganizationId);

        await clientA.PostAsJsonAsync(
            "/api/teams",
            new { name = "Team A", description = "A" });

        using var clientB = factory.CreateClient();
        var organizationB = await RegisterAndLoginAsync(
            clientB, "ownerB@test.com", "Organization B");
        SelectOrganization(clientB, organizationB.OrganizationId);

        await clientB.PostAsJsonAsync(
            "/api/teams",
            new { name = "Team B", description = "B" });

        var response = await clientA.GetAsync("/api/teams");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var teams = await response.Content
            .ReadFromJsonAsync<List<Dictionary<string, object>>>();

        Assert.NotNull(teams);
        Assert.Single(teams);
        Assert.Contains("Team A", teams[0].Values.Select(v => v?.ToString()));
        Assert.DoesNotContain(
            teams,
            team => team.Values.Any(v => v?.ToString() == "Team B"));
    }
}
