using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiCore.Infrastructure.Persistence;

namespace ServiCore.IntegrationTests.Infrastructure;

public class IntegrationTestWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private const string ConnectionString =
        "Server=.;Database=ServiCoreDb_IntegrationTests;" +
        "Trusted_Connection=True;TrustServerCertificate=True;";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<ServiCoreDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<ServiCoreDbContext>(options =>
                options.UseSqlServer(ConnectionString));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ServiCoreDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }
}