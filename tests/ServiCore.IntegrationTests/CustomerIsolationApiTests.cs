using System.Net;
using System.Net.Http.Json;
using ServiCore.IntegrationTests.Common;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public sealed class CustomerIsolationApiTests : IntegrationTestBase
{
    [Fact]
    public async Task Customer_ShouldSeeOnlyOwnTickets()
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
        var customerA = await CreateCustomerAsync(ownerClient, "Customer A", "customer.a@test.com");
        var customerB = await CreateCustomerAsync(ownerClient, "Customer B", "customer.b@test.com");

        var invitationA = await InviteCustomerAsync(ownerClient, customerA.Id);
        var invitationB = await InviteCustomerAsync(ownerClient, customerB.Id);

        using var anonymousClient = factory.CreateClient();
        var acceptA = await anonymousClient.PostAsJsonAsync(
            "/api/customer-invitations/accept",
            new { token = invitationA, password = "Password123!" });
        Assert.Equal(HttpStatusCode.OK, acceptA.StatusCode);

        var acceptB = await anonymousClient.PostAsJsonAsync(
            "/api/customer-invitations/accept",
            new { token = invitationB, password = "Password123!" });
        Assert.Equal(HttpStatusCode.OK, acceptB.StatusCode);

        var ticketA = await CreateTicketAsync(
            ownerClient,
            customerA.Id,
            teamId,
            categoryId,
            "Ticket A");

        _ = await CreateTicketAsync(
            ownerClient,
            customerB.Id,
            teamId,
            categoryId,
            "Ticket B");

        using var customerClient = factory.CreateClient();
        await LoginExistingUserAsync(
            customerClient,
            "customer.a@test.com",
            "Password123!");
        SelectOrganization(customerClient, organization.OrganizationId);

        var response = await customerClient.GetAsync("/api/tickets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tickets = await response.Content
            .ReadFromJsonAsync<List<TicketTestResponse>>();

        Assert.NotNull(tickets);
        Assert.Single(tickets);
        Assert.Equal(ticketA.Id, tickets[0].Id);
        Assert.Equal(customerA.Id, tickets[0].CustomerId);
    }

    [Fact]
    public async Task Customer_ShouldNotReadAnotherCustomersTicketById()
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
        var customerA = await CreateCustomerAsync(ownerClient, "Customer A", "customer.a@test.com");
        var customerB = await CreateCustomerAsync(ownerClient, "Customer B", "customer.b@test.com");

        var invitationA = await InviteCustomerAsync(ownerClient, customerA.Id);
        using var anonymousClient = factory.CreateClient();
        var acceptA = await anonymousClient.PostAsJsonAsync(
            "/api/customer-invitations/accept",
            new { token = invitationA, password = "Password123!" });
        Assert.Equal(HttpStatusCode.OK, acceptA.StatusCode);

        var ticketB = await CreateTicketAsync(
            ownerClient,
            customerB.Id,
            teamId,
            categoryId,
            "Private Ticket B");

        using var customerClient = factory.CreateClient();
        await LoginExistingUserAsync(
            customerClient,
            "customer.a@test.com",
            "Password123!");
        SelectOrganization(customerClient, organization.OrganizationId);

        var response = await customerClient.GetAsync(
            $"/api/tickets/{ticketB.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<Guid> CreateTeamAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/teams",
            new { name = "Support Team", description = "Support." });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var team = await response.Content.ReadFromJsonAsync<TeamTestResponse>();
        Assert.NotNull(team);
        return team.Id;
    }

    private static async Task<Guid> CreateCategoryAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/categories",
            new { name = "Technical", description = "Technical issues." });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var category = await response.Content.ReadFromJsonAsync<CategoryTestResponse>();
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
            new { name, email, phoneNumber = "01000000000" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomerTestResponse>();
        Assert.NotNull(customer);
        return customer;
    }

    private static async Task<string> InviteCustomerAsync(
        HttpClient client,
        Guid customerId)
    {
        var response = await client.PostAsJsonAsync(
            "/api/customer-invitations",
            new { customerId });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        return body["token"];
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
                priority = 2
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var ticket = await response.Content.ReadFromJsonAsync<TicketTestResponse>();
        Assert.NotNull(ticket);
        return ticket;
    }

    private static async Task LoginExistingUserAsync(
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
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login.Token);
    }
}
