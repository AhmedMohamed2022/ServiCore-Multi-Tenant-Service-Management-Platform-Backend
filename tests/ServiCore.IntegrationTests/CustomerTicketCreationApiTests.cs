using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using ServiCore.Domain.Enums;
using ServiCore.IntegrationTests.Common;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public sealed class CustomerTicketCreationApiTests : IntegrationTestBase
{
    [Fact]
    public async Task Customer_ShouldCreateUnassignedTicket()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();

        using var ownerClient = factory.CreateClient();

        var organization = await RegisterAndLoginAsync(
            ownerClient,
            "owner@test.com",
            "Organization A");

        SelectOrganization(ownerClient, organization.OrganizationId);

        var teamResponse = await ownerClient.PostAsJsonAsync(
            "/api/teams",
            new
            {
                name = "Support",
                description = "Support team."
            });

        Assert.Equal(HttpStatusCode.OK, teamResponse.StatusCode);

        var team = await teamResponse.Content
            .ReadFromJsonAsync<TeamTestResponse>();

        Assert.NotNull(team);

        var categoryResponse = await ownerClient.PostAsJsonAsync(
            "/api/categories",
            new
            {
                name = "Technical",
                description = "Technical issues."
            });

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var category = await categoryResponse.Content
            .ReadFromJsonAsync<CategoryTestResponse>();

        Assert.NotNull(category);

        var customerResponse = await ownerClient.PostAsJsonAsync(
            "/api/customers",
            new
            {
                name = "Customer",
                email = "customer@test.com",
                phoneNumber = "01000000000"
            });

        Assert.Equal(HttpStatusCode.Created, customerResponse.StatusCode);

        var customer = await customerResponse.Content
            .ReadFromJsonAsync<CustomerTestResponse>();

        Assert.NotNull(customer);

        var invitationToken = await InviteCustomerAsync(
            ownerClient,
            factory,
            customer.Id,
            "customer@test.com");

        using var anonymousClient = factory.CreateClient();

        var acceptResponse = await anonymousClient.PostAsJsonAsync(
            "/api/customer-invitations/accept",
            new
            {
                token = invitationToken,
                password = "Password123!"
            });

        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);

        using var customerClient = factory.CreateClient();

        await LoginAsync(
            customerClient,
            "customer@test.com",
            "Password123!");

        SelectOrganization(
            customerClient,
            organization.OrganizationId);

        var createResponse = await customerClient.PostAsJsonAsync(
            "/api/customer-tickets",
            new
            {
                categoryId = category.Id,
                title = "Cannot login",
                description = "The customer cannot access the portal.",
                priority = TicketPriority.High
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var ticket = await createResponse.Content
            .ReadFromJsonAsync<TicketTestResponse>();

        Assert.NotNull(ticket);
        Assert.Equal(customer.Id, ticket.CustomerId);
        Assert.Null(ticket.TeamId);
        Assert.Null(ticket.AssignedAgentId);
        Assert.Equal(TicketStatus.New, (TicketStatus)ticket.Status);

        var notificationsResponse =
            await ownerClient.GetAsync("/api/notifications");

        Assert.Equal(
            HttpStatusCode.OK,
            notificationsResponse.StatusCode);

        var notifications =
            await notificationsResponse.Content
                .ReadFromJsonAsync<List<NotificationTestResponse>>();

        Assert.NotNull(notifications);
        Assert.Contains(
            notifications,
            notification =>
                notification.Type == (int)NotificationType.TicketCreated &&
                notification.RelatedEntityId == ticket.Id);

        var assignTeamResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/tickets/{ticket.Id}/assign-team",
                new
                {
                    teamId = team.Id
                });

        Assert.Equal(
            HttpStatusCode.OK,
            assignTeamResponse.StatusCode);

        var assigned =
            await assignTeamResponse.Content
                .ReadFromJsonAsync<TicketTestResponse>();

        Assert.NotNull(assigned);
        Assert.Equal(team.Id, assigned.TeamId);
        Assert.Equal(TicketStatus.Open, (TicketStatus)assigned.Status);
        Assert.Null(assigned.AssignedAgentId);
    }

    [Fact]
    public async Task Customer_ShouldNotCreateStaffTicket()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();

        using var ownerClient = factory.CreateClient();

        var organization = await RegisterAndLoginAsync(
            ownerClient,
            "owner@test.com",
            "Organization A");

        SelectOrganization(ownerClient, organization.OrganizationId);

        var teamId = await CreateTeamAsync(ownerClient);
        var categoryId = await CreateCategoryAsync(ownerClient);

        var customerResponse = await ownerClient.PostAsJsonAsync(
            "/api/customers",
            new
            {
                name = "Customer",
                email = "customer@test.com",
                phoneNumber = "01000000000"
            });

        Assert.Equal(HttpStatusCode.Created, customerResponse.StatusCode);

        var customer =
            await customerResponse.Content
                .ReadFromJsonAsync<CustomerTestResponse>();

        Assert.NotNull(customer);

        var invitationToken = await InviteCustomerAsync(
            ownerClient,
            factory,
            customer.Id,
            "customer@test.com");

        using var anonymousClient = factory.CreateClient();

        var acceptResponse = await anonymousClient.PostAsJsonAsync(
            "/api/customer-invitations/accept",
            new
            {
                token = invitationToken,
                password = "Password123!"
            });

        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);

        using var customerClient = factory.CreateClient();

        await LoginAsync(
            customerClient,
            "customer@test.com",
            "Password123!");

        SelectOrganization(
            customerClient,
            organization.OrganizationId);

        var response = await customerClient.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                customerId = customer.Id,
                teamId,
                categoryId,
                title = "Must fail",
                description = "Customers use the customer ticket endpoint.",
                priority = TicketPriority.Medium
            });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    private static async Task<Guid> CreateTeamAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/teams",
            new
            {
                name = "Support",
                description = "Support team."
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var team =
            await response.Content
                .ReadFromJsonAsync<TeamTestResponse>();

        Assert.NotNull(team);

        return team.Id;
    }

    private static async Task<Guid> CreateCategoryAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/categories",
            new
            {
                name = "Technical",
                description = "Technical issues."
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var category =
            await response.Content
                .ReadFromJsonAsync<CategoryTestResponse>();

        Assert.NotNull(category);

        return category.Id;
    }

    private static async Task<string> InviteCustomerAsync(
        HttpClient client,
        IntegrationTestWebApplicationFactory factory,
        Guid customerId,
        string expectedEmail)
    {
        factory.TestEmails.Clear();

        var response = await client.PostAsJsonAsync(
            "/api/customer-invitations",
            new
            {
                customerId
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var email =
            Assert.Single(factory.TestEmails.Messages);

        Assert.Equal(
            expectedEmail,
            email.RecipientEmail,
            ignoreCase: true);

        return ExtractInvitationToken(email.HtmlBody);
    }

    private static string ExtractInvitationToken(string htmlBody)
    {
        var match = Regex.Match(
            htmlBody,
            @"[?&]token=([^&""'\s<]+)",
            RegexOptions.IgnoreCase);

        Assert.True(
            match.Success,
            "The invitation email did not contain an invitation token.");

        return Uri.UnescapeDataString(match.Groups[1].Value);
    }

    private static async Task LoginAsync(
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

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var login =
            await response.Content
                .ReadFromJsonAsync<LoginTestResponse>();

        Assert.NotNull(login);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                login.Token);
    }

    private sealed record NotificationTestResponse(
        Guid Id,
        int Type,
        string Title,
        string Message,
        Guid? RelatedEntityId,
        DateTime CreatedAt,
        DateTime? ReadAt,
        bool IsRead);
}
