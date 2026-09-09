using Microsoft.EntityFrameworkCore;
using ServiCore.API.Hubs;
using ServiCore.API.Middleware;
using ServiCore.Application;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Infrastructure;
using ServiCore.Infrastructure.Persistence;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(
                builder.Configuration);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSignalR();
        
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        builder.Services.AddScoped<INotificationRealtimePublisher, NotificationRealtimePublisher>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthentication();
        app.UseMiddleware<TenantResolutionMiddleware>();

        app.UseAuthorization();


        app.MapControllers();
        app.MapHub<NotificationHub>("/hubs/notifications");
        app.Run();

    }
}
public partial class Program
{
}

