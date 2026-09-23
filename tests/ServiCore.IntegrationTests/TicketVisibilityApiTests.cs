using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using ServiCore.Domain.Enums;
using ServiCore.IntegrationTests.Common;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public sealed class TicketVisibilityApiTests : IntegrationTestBase
{
    [Fact]
    public async Task Manager_ShouldSeeUnassignedAndOwnTeamTicketsOnly()
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

        var teamA = await CreateTeamAsync(
            ownerClient,
            "Technical");

        var teamB = await CreateTeamAsync(
            ownerClient,
            "Billing");

        var categoryId = await CreateCategoryAsync(ownerClient);

        var customerA = await CreateCustomerAsync(
            ownerClient,
            "Customer A",
            "customer.a@test.com");

        var customerB = await CreateCustomerAsync(
            ownerClient,
            "Customer B",
            "customer.b@test.com");

        var managerToken = await InviteOrganizationMemberAsync(
            ownerClient,
            factory,
            "manager@test.com",
            OrganizationRole.Manager);

        using var anonymousClient = factory.CreateClient();

        var acceptResponse = await anonymousClient.PostAsJsonAsync(
            "/api/organization/invitations/accept",
            new
            {
                token = managerToken,
                password = "Password123!"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            acceptResponse.StatusCode);

        using var managerClient = factory.CreateClient();

        await LoginAsync(
            managerClient,
            "manager@test.com",
            "Password123!");

        SelectOrganization(
            managerClient,
            organization.OrganizationId);

        var meResponse = await managerClient.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me =
            await meResponse.Content
                .ReadFromJsonAsync<Dictionary<string, string>>();

        Assert.NotNull(me);
        var managerUserId = Guid.Parse(me["userId"]);

        var addMemberResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/teams/{teamA.Id}/members",
                new
                {
                    userId = managerUserId
                });

        Assert.Equal(
            HttpStatusCode.NoContent,
            addMemberResponse.StatusCode);

        var teamTicket = await CreateTicketAsync(
            ownerClient,
            customerA.Id,
            teamA.Id,
            categoryId,
            "Technical ticket");

        var otherTeamTicket = await CreateTicketAsync(
            ownerClient,
            customerB.Id,
            teamB.Id,
            categoryId,
            "Billing ticket");

        var customerUnassigned = await CreateCustomerAsync(
            ownerClient,
            "Customer C",
            "customer.c@test.com");

        var unassignedResponse =
            await ownerClient.PostAsJsonAsync(
                "/api/customer-tickets",
                new
                {
                    categoryId,
                    title = "Customer intake ticket",
                    description = "Needs triage.",
                    priority = TicketPriority.Medium
                });

        // The owner is not the customer linked to this request, so the
        // customer endpoint must reject the request. This also keeps this
        // test focused on manager visibility.
        Assert.Equal(
            HttpStatusCode.Forbidden,
            unassignedResponse.StatusCode);

        // Create an unassigned ticket directly through the application
        // database so the visibility query can be tested without introducing
        // another customer authentication flow into this test.
        // The actual customer endpoint is covered separately.
        var unassignedTicketId = await CreateUnassignedTicketInDatabaseAsync(
            factory,
            organization.OrganizationId,
            customerUnassigned.Id,
            categoryId);

        var response = await managerClient.GetAsync("/api/tickets");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var tickets =
            await response.Content
                .ReadFromJsonAsync<List<TicketTestResponse>>();

        Assert.NotNull(tickets);

        Assert.Contains(
            tickets,
            ticket => ticket.Id == teamTicket.Id);

        Assert.DoesNotContain(
            tickets,
            ticket => ticket.Id == otherTeamTicket.Id);

        Assert.Contains(
            tickets,
            ticket => ticket.Id == unassignedTicketId);
    }

    private static async Task<TeamTestResponse> CreateTeamAsync(
        HttpClient client,
        string name)
    {
        var response = await client.PostAsJsonAsync(
            "/api/teams",
            new
            {
                name,
                description = $"{name} team."
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var team =
            await response.Content
                .ReadFromJsonAsync<TeamTestResponse>();

        Assert.NotNull(team);

        return team;
    }

    private static async Task<Guid> CreateCategoryAsync(
        HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/categories",
            new
            {
                name = "Technical",
                description = "Technical issues."
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var category =
            await response.Content
                .ReadFromJsonAsync<CategoryTestResponse>();

        Assert.NotNull(category);

        return category.Id;
    }

    private static async Task<CustomerTestResponse> CreateCustomerAsync(
        HttpClient client,
        string name,
        string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/customers",
            new
            {
                name,
                email,
                phoneNumber = "01000000000"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var customer =
            await response.Content
                .ReadFromJsonAsync<CustomerTestResponse>();

        Assert.NotNull(customer);

        return customer;
    }

    private static async Task<TicketTestResponse> CreateTicketAsync(
        HttpClient client,
        Guid customerId,
        Guid teamId,
        Guid categoryId,
        string title)
    {
        var response = await client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                customerId,
                teamId,
                categoryId,
                title,
                description = "Test ticket description.",
                priority = TicketPriority.Medium
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var ticket =
            await response.Content
                .ReadFromJsonAsync<TicketTestResponse>();

        Assert.NotNull(ticket);

        return ticket;
    }

    private static async Task<Guid> CreateUnassignedTicketInDatabaseAsync(
        IntegrationTestWebApplicationFactory factory,
        Guid organizationId,
        Guid customerId,
        Guid categoryId)
    {
        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ServiCore.Infrastructure.Persistence.ServiCoreDbContext>();

        var ticket = new ServiCore.Domain.Entities.Ticket(
            organizationId,
            customerId,
            null,
            categoryId,
            "Unassigned ticket",
            "Needs triage.",
            TicketPriority.High);

        db.Tickets.Add(ticket);

        await db.SaveChangesAsync();

        return ticket.Id;
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

        var sentEmail =
            Assert.Single(factory.TestEmails.Messages);

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

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var login =
            await response.Content
                .ReadFromJsonAsync<LoginTestResponse>();

        Assert.NotNull(login);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                login.Token);
    }
}
