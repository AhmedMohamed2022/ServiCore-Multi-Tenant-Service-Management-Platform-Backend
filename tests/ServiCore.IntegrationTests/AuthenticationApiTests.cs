using System.Net;
using System.Net.Http.Json;
using ServiCore.IntegrationTests.Common;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public sealed class AuthenticationApiTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_ShouldCreateUserAndOrganization()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                name = "Owner",
                email = "owner@test.com",
                password = "Password123!",
                organizationName = "Organization A"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<RegisterTestResponse>();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.NotEqual(Guid.Empty, result.OrganizationId);
        Assert.Equal("Organization A", result.OrganizationName);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        await RegisterAndLoginAsync(client, "owner@test.com", "Organization A");
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "owner@test.com",
                password = "WrongPassword123!"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutAuthentication_ShouldReturn401()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ShouldReturnCurrentUser()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var registration = await RegisterAndLoginAsync(
            client,
            "owner@test.com",
            "Organization A");

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<Dictionary<string, string>>();

        Assert.NotNull(body);
        Assert.Equal(registration.UserId.ToString(), body["userId"]);
        Assert.Equal("owner@test.com", body["email"]);
    }
}
