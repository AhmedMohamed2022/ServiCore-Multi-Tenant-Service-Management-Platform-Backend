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
    public async Task StandaloneOrganizationCreationEndpoint_ShouldNotExist()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        await factory.ResetDatabaseAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/organizations",
            new { name = "Acme Support" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
