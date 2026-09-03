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

public class TeamsApiTests
{
    [Fact]
    public async Task CreateTeam_WithAuthenticatedUserAndValidTenant_ShouldSucceed()
    {
        await using var factory =
            new IntegrationTestWebApplicationFactory();

        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient();

        // Register
        var registerResponse =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                new
                {
                    name = "Ahmed",
                    email = "owner@test.com",
                    password = "Password123!",
                    organizationName = "Organization A"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        var registerResult =
            await registerResponse.Content
                .ReadFromJsonAsync<RegisterTestResponse>();

        Assert.NotNull(registerResult);

        // Login
        var loginResponse =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    email = "owner@test.com",
                    password = "Password123!"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginTestResponse>();

        Assert.NotNull(loginResult);
        Assert.False(
            string.IsNullOrWhiteSpace(loginResult.Token));

        // Authenticate
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.Token);

        // Select tenant
        client.DefaultRequestHeaders.Add(
            "X-Organization-Id",
            registerResult.OrganizationId.ToString());

        // Create team
        var teamResponse =
            await client.PostAsJsonAsync(
                "/api/teams",
                new
                {
                    name = "Support Team",
                    description = "Handles customer support."
                });

        Assert.Equal(
            HttpStatusCode.OK,
            teamResponse.StatusCode);

        var team =
            await teamResponse.Content
                .ReadFromJsonAsync<TeamTestResponse>();

        Assert.NotNull(team);

        Assert.Equal(
            registerResult.OrganizationId,
            team.OrganizationId);

        Assert.Equal(
            "Support Team",
            team.Name);

        Assert.Equal(
            "Handles customer support.",
            team.Description);
    }
}