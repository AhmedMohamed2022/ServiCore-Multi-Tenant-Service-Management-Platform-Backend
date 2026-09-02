using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ServiCore.Application.Organizations.DTOs;
using ServiCore.Infrastructure.Persistence;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public class OrganizationsApiTests
{
    [Fact]
    public async Task CreateOrganization_Should_Persist_Organization()
    {
        await using var factory =
            new IntegrationTestWebApplicationFactory();

        await factory.ResetDatabaseAsync();

        using var client = factory.CreateClient();

        var request = new CreateOrganizationRequest(
            "Acme Support");

        var response = await client.PostAsJsonAsync(
            "/api/organizations",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var createdOrganization =
            await response.Content
                .ReadFromJsonAsync<OrganizationDto>();

        Assert.NotNull(createdOrganization);
        Assert.Equal("Acme Support", createdOrganization.Name);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ServiCoreDbContext>();

        var persistedOrganization =
            await dbContext.Organizations.FindAsync(
                createdOrganization.Id);

        Assert.NotNull(persistedOrganization);

        Assert.Equal(
            "Acme Support",
            persistedOrganization.Name);
    }
}