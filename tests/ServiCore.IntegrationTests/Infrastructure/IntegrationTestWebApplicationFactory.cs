using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Infrastructure.Persistence;

namespace ServiCore.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebApplicationFactory
    : WebApplicationFactory<Program>
{
    public const string ConnectionString =
        "Server=.;Database=ServiCoreDb_IntegrationTests;" +
        "Trusted_Connection=True;TrustServerCertificate=True;";

    public static readonly string TestJwtSecret =
        "IntegrationTestsOnly-ServiCore-Secret-Key-1234567890!";
    public TestEmailSender TestEmails =>
    Services.GetRequiredService<TestEmailSender>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT_SECRET_KEY"] = TestJwtSecret,
                ["Frontend:BaseUrl"] = "http://localhost:4200"
            });
        });

        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ServiCoreDbContext>));

            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            services.AddDbContext<ServiCoreDbContext>(options =>
                options.UseSqlServer(ConnectionString));

            var emailDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IEmailSender));

            if (emailDescriptor is not null)
                services.Remove(emailDescriptor);

            services.AddSingleton<TestEmailSender>();

            services.AddSingleton<IEmailSender>(
                provider =>
                    provider.GetRequiredService<TestEmailSender>());
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

public sealed class TestEmailSender : IEmailSender
{
    private readonly List<SentEmail> _messages = new();

    public IReadOnlyList<SentEmail> Messages => _messages;

    public Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        _messages.Add(new SentEmail(
            recipientEmail,
            subject,
            htmlBody));

        return Task.CompletedTask;
    }
    public void Clear()
    {
        _messages.Clear();
    }
}

public sealed record SentEmail(
    string RecipientEmail,
    string Subject,
    string HtmlBody);
