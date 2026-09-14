using System.Net;
using System.Net.Http.Json;
using ServiCore.Domain.Enums;
using ServiCore.IntegrationTests.Common;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public sealed class TicketWorkflowApiTests : IntegrationTestBase
{
    [Fact]
    public async Task Owner_ShouldCreateAndOpenTicket()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var organization = await RegisterAndLoginAsync(
            client,
            "owner@test.com",
            "Organization A");
        SelectOrganization(client, organization.OrganizationId);

        var teamResponse = await client.PostAsJsonAsync(
            "/api/teams",
            new { name = "Support", description = "Support team." });
        Assert.Equal(HttpStatusCode.OK, teamResponse.StatusCode);
        var team = await teamResponse.Content.ReadFromJsonAsync<TeamTestResponse>();
        Assert.NotNull(team);

        var categoryResponse = await client.PostAsJsonAsync(
            "/api/categories",
            new { name = "Technical", description = "Technical issues." });
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryTestResponse>();
        Assert.NotNull(category);

        var customerResponse = await client.PostAsJsonAsync(
            "/api/customers",
            new { name = "Customer", email = "customer@test.com", phoneNumber = "01000000000" });
        Assert.Equal(HttpStatusCode.Created, customerResponse.StatusCode);
        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerTestResponse>();
        Assert.NotNull(customer);

        var createResponse = await client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                customerId = customer.Id,
                teamId = team.Id,
                categoryId = category.Id,
                title = "Cannot login",
                description = "The customer cannot log in.",
                priority = TicketPriority.High
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var ticket = await createResponse.Content.ReadFromJsonAsync<TicketTestResponse>();
        Assert.NotNull(ticket);
        Assert.Equal(1, ticket.Status);
        Assert.Equal(organization.OrganizationId, ticket.OrganizationId);

        var openResponse = await client.PostAsync(
            $"/api/tickets/{ticket.Id}/open",
            null);

        Assert.Equal(HttpStatusCode.OK, openResponse.StatusCode);
        var opened = await openResponse.Content.ReadFromJsonAsync<TicketTestResponse>();
        Assert.NotNull(opened);
        Assert.Equal(2, opened.Status);
    }
}
