using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ServiCore.IntegrationTests.Common;

namespace ServiCore.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
{
    protected static async Task<OrganizationTestResponse> RegisterAndLoginAsync(
        HttpClient client,
        string email,
        string organizationName,
        string password = "Password123!")
    {
        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                name = "Test User",
                email,
                password,
                organizationName
            });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registration = await registerResponse.Content
            .ReadFromJsonAsync<OrganizationTestResponse>();

        Assert.NotNull(registration);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var login = await loginResponse.Content
            .ReadFromJsonAsync<LoginTestResponse>();

        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);

        return registration;
    }

    protected static void SelectOrganization(
        HttpClient client,
        Guid organizationId)
    {
        client.DefaultRequestHeaders.Remove("X-Organization-Id");
        client.DefaultRequestHeaders.Add(
            "X-Organization-Id",
            organizationId.ToString());
    }
}
