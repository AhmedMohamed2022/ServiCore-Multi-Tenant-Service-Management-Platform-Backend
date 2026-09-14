using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiCore.Application.Organizations.DTOs;
using ServiCore.Infrastructure.Persistence;
using ServiCore.IntegrationTests.Infrastructure;

namespace ServiCore.IntegrationTests;

public sealed class OrganizationsApiTests
{
    [Fact]
    public async Task CreateOrganization_ShouldPersistOrganization()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/organizations",
            new CreateOrganizationRequest("Acme Support"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<OrganizationDto>();

        Assert.NotNull(created);
        Assert.Equal("Acme Support", created.Name);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<ServiCoreDbContext>();

        var persisted = await db.Organizations
            .SingleAsync(x => x.Id == created.Id);

        Assert.Equal("Acme Support", persisted.Name);
    }
}
