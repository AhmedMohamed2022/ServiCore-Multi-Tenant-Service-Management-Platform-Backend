using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiCore.Infrastructure.Persistence;
using ServiCore.IntegrationTests.Common;
using ServiCore.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace ServiCore.IntegrationTests;

public class TenantSecurityApiTests
{
    [Fact]
    public async Task CreateTeam_WithoutAuthentication_ShouldReturn401()
    {
        await using var factory =
            new IntegrationTestWebApplicationFactory();

        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "/api/teams",
                new
                {
                    name = "Unauthorized Team",
                    description = "Should not be created."
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
    public static async Task<OrganizationTestResponse>
    RegisterAndLoginAsync(
        HttpClient client,
        string email,
        string organizationName)
    {
        var registerResponse =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                new
                {
                    name = "Test User",
                    email,
                    password = "Password123!",
                    organizationName
                });

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        var registration =
            await registerResponse.Content
                .ReadFromJsonAsync<OrganizationTestResponse>();

        Assert.NotNull(registration);

        var loginResponse =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    email,
                    password = "Password123!"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var login =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginTestResponse>();

        Assert.NotNull(login);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                login.Token);

        return registration;
    }
    [Fact]
    public async Task CreateTeam_WithoutTenantHeader_ShouldReturn400()
    {
        await using var factory =
            new IntegrationTestWebApplicationFactory();

        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient();

        await RegisterAndLoginAsync(
            client,
            "owner@test.com",
            "Organization A");

        var response =
            await client.PostAsJsonAsync(
                "/api/teams",
                new
                {
                    name = "Support Team",
                    description = "Handles support."
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    [Fact]
    public async Task CreateTeam_WithNonMemberOrganization_ShouldReturn403()
    {
        await using var factory =
            new IntegrationTestWebApplicationFactory();

        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient();

        // Create Organization A
        var organizationA =
            await RegisterAndLoginAsync(
                client,
                "ownerA@test.com",
                "Organization A");

        // Create Organization B using another user/client
        using var clientB = factory.CreateClient();

        var organizationB =
            await RegisterAndLoginAsync(
                clientB,
                "ownerB@test.com",
                "Organization B");

        // Use User A's JWT
        client.DefaultRequestHeaders.Remove(
            "X-Organization-Id");

        client.DefaultRequestHeaders.Add(
            "X-Organization-Id",
            organizationB.OrganizationId.ToString());

        var response =
            await client.PostAsJsonAsync(
                "/api/teams",
                new
                {
                    name = "Cross Tenant Team",
                    description = "Must not be created."
                });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }
    [Fact]
    public async Task CreateTeam_ShouldPersistTeamInsideSelectedTenant()
    {
        await using var factory =
            new IntegrationTestWebApplicationFactory();

        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient();

        var organization =
            await RegisterAndLoginAsync(
                client,
                "owner@test.com",
                "Organization A");

        client.DefaultRequestHeaders.Add(
            "X-Organization-Id",
            organization.OrganizationId.ToString());

        var response =
            await client.PostAsJsonAsync(
                "/api/teams",
                new
                {
                    name = "Support Team",
                    description = "Handles customer support."
                });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var team =
            await response.Content
                .ReadFromJsonAsync<TeamTestResponse>();

        Assert.NotNull(team);

        using var scope =
            factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<ServiCoreDbContext>();

        var storedTeam =
            await dbContext.Teams
                .SingleAsync(x => x.Id == team.Id);

        Assert.Equal(
            organization.OrganizationId,
            storedTeam.OrganizationId);
    }
}
