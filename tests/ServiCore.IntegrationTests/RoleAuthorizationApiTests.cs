using System.Net;
using System.Net.Http.Json;
using ServiCore.Domain.Enums;
using ServiCore.IntegrationTests.Common;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public sealed class RoleAuthorizationApiTests : IntegrationTestBase
{
    [Fact]
    public async Task Agent_ShouldNotCreateTeam()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();

        using var ownerClient = factory.CreateClient();
        var organization = await RegisterAndLoginAsync(
            ownerClient,
            "owner@test.com",
            "Organization A");
        SelectOrganization(ownerClient, organization.OrganizationId);

        var inviteResponse = await ownerClient.PostAsJsonAsync(
            "/api/organization/invitations",
            new
            {
                email = "agent@test.com",
                role = OrganizationRole.Agent
            });

        Assert.Equal(HttpStatusCode.OK, inviteResponse.StatusCode);

        var invitation = await inviteResponse.Content.ReadFromJsonAsync<InvitationResponse>();
        Assert.NotNull(invitation);
        Assert.False(string.IsNullOrWhiteSpace(invitation.InvitationToken));

        using var anonymousClient = factory.CreateClient();
        var acceptResponse = await anonymousClient.PostAsJsonAsync(
            "/api/organization/invitations/accept",
            new
            {
                token = invitation.InvitationToken,
                password = "Password123!"
            });

        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);

        using var agentClient = factory.CreateClient();
        await RegisterAndLoginExistingUserAsync(
            agentClient,
            "agent@test.com",
            "Password123!");
        SelectOrganization(agentClient, organization.OrganizationId);

        var response = await agentClient.PostAsJsonAsync(
            "/api/teams",
            new { name = "Agent Team", description = "Must fail." });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Agent_ShouldNotInviteAnotherOrganizationMember()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();

        using var ownerClient = factory.CreateClient();
        var organization = await RegisterAndLoginAsync(
            ownerClient,
            "owner@test.com",
            "Organization A");
        SelectOrganization(ownerClient, organization.OrganizationId);

        var inviteResponse = await ownerClient.PostAsJsonAsync(
            "/api/organization/invitations",
            new { email = "agent@test.com", role = OrganizationRole.Agent });
        Assert.Equal(HttpStatusCode.OK, inviteResponse.StatusCode);

        var invitation = await inviteResponse.Content.ReadFromJsonAsync<InvitationResponse>();
        Assert.NotNull(invitation);

        using var anonymousClient = factory.CreateClient();
        var acceptResponse = await anonymousClient.PostAsJsonAsync(
            "/api/organization/invitations/accept",
            new { token = invitation.InvitationToken, password = "Password123!" });
        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);

        using var agentClient = factory.CreateClient();
        await RegisterAndLoginExistingUserAsync(agentClient, "agent@test.com", "Password123!");
        SelectOrganization(agentClient, organization.OrganizationId);

        var response = await agentClient.PostAsJsonAsync(
            "/api/organization/invitations",
            new { email = "another@test.com", role = OrganizationRole.Agent });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task RegisterAndLoginExistingUserAsync(
        HttpClient client,
        string email,
        string password)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var login = await response.Content.ReadFromJsonAsync<LoginTestResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login.Token);
    }

    private sealed record InvitationResponse(
        InvitationSummary Invitation,
        string InvitationToken);

    private sealed record InvitationSummary(
        Guid Id,
        Guid OrganizationId,
        string Email,
        string Role,
        DateTime CreatedAt,
        DateTime ExpiresAt,
        bool IsAccepted,
        bool IsRevoked);
}
