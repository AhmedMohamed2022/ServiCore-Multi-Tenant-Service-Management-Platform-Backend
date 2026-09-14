using Microsoft.OpenApi.Models;
using ServiCore.API.Hubs;
using ServiCore.API.Middleware;
using ServiCore.Application;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Infrastructure;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplication();

        builder.Services.AddInfrastructure(
            builder.Configuration);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description =
                        "Enter your JWT token."
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference =
                                new OpenApiReference
                                {
                                    Type =
                                        ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                        },
                        Array.Empty<string>()
                    }
                });
        });

        builder.Services.AddSignalR();

        builder.Services.AddProblemDetails();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var allowedOrigins =
                    builder.Configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>();

                if (allowedOrigins is null ||
                    allowedOrigins.Length == 0)
                {
                    throw new InvalidOperationException(
                        "At least one CORS allowed origin must be configured.");
                }

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        builder.Services.AddScoped<
            INotificationRealtimePublisher,
            NotificationRealtimePublisher>();

        builder.Services.AddScoped<
            ITicketCommentRealtimePublisher,
            TicketCommentRealtimePublisher>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
        }

        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.UseCors();

        app.UseAuthentication();

        app.UseMiddleware<TenantResolutionMiddleware>();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<NotificationHub>(
            "/hubs/notifications");

        app.Run();
    }
}