using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
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

        SelectOrganization(
            ownerClient,
            organization.OrganizationId);

        var invitationToken = await InviteOrganizationMemberAsync(
            ownerClient,
            factory,
            "agent@test.com",
            OrganizationRole.Agent);

        using var anonymousClient = factory.CreateClient();

        var acceptResponse = await anonymousClient.PostAsJsonAsync(
            "/api/organization/invitations/accept",
            new
            {
                token = invitationToken,
                password = "Password123!"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            acceptResponse.StatusCode);

        using var agentClient = factory.CreateClient();

        await RegisterAndLoginExistingUserAsync(
            agentClient,
            "agent@test.com",
            "Password123!");

        SelectOrganization(
            agentClient,
            organization.OrganizationId);

        var response = await agentClient.PostAsJsonAsync(
            "/api/teams",
            new
            {
                name = "Agent Team",
                description = "Must fail."
            });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
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

        SelectOrganization(
            ownerClient,
            organization.OrganizationId);

        var invitationToken = await InviteOrganizationMemberAsync(
            ownerClient,
            factory,
            "agent@test.com",
            OrganizationRole.Agent);

        using var anonymousClient = factory.CreateClient();

        var acceptResponse = await anonymousClient.PostAsJsonAsync(
            "/api/organization/invitations/accept",
            new
            {
                token = invitationToken,
                password = "Password123!"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            acceptResponse.StatusCode);

        using var agentClient = factory.CreateClient();

        await RegisterAndLoginExistingUserAsync(
            agentClient,
            "agent@test.com",
            "Password123!");

        SelectOrganization(
            agentClient,
            organization.OrganizationId);

        var response = await agentClient.PostAsJsonAsync(
            "/api/organization/invitations",
            new
            {
                email = "another@test.com",
                role = OrganizationRole.Agent
            });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    private static async Task<string> InviteOrganizationMemberAsync(
        HttpClient client,
        IntegrationTestWebApplicationFactory factory,
        string email,
        OrganizationRole role)
    {
        factory.TestEmails.Clear();

        var response = await client.PostAsJsonAsync(
            "/api/organization/invitations",
            new
            {
                email,
                role
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var sentEmail = Assert.Single(factory.TestEmails.Messages);

        Assert.Equal(
            email,
            sentEmail.RecipientEmail,
            ignoreCase: true);

        Assert.False(
            string.IsNullOrWhiteSpace(sentEmail.HtmlBody));

        return ExtractInvitationToken(
            sentEmail.HtmlBody);
    }

    private static string ExtractInvitationToken(
        string htmlBody)
    {
        var match = Regex.Match(
            htmlBody,
            @"[?&]token=([^&""'\s<]+)",
            RegexOptions.IgnoreCase);

        Assert.True(
            match.Success,
            "The invitation email did not contain an invitation token.");

        return Uri.UnescapeDataString(
            match.Groups[1].Value);
    }

    private static async Task RegisterAndLoginExistingUserAsync(
        HttpClient client,
        string email,
        string password)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email,
                password
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var login =
            await response.Content
                .ReadFromJsonAsync<LoginTestResponse>();

        Assert.NotNull(login);

        Assert.False(
            string.IsNullOrWhiteSpace(login.Token));

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                login.Token);
    }
}

