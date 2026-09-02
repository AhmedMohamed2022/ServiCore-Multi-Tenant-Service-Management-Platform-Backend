using Microsoft.EntityFrameworkCore;
using ServiCore.Infrastructure.Persistence;
using ServiCore.Infrastructure;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructure(
                builder.Configuration);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();
        //builder.Services.AddHttpsRedirection(options =>
        //options.HttpsPort = 7059);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();
        app.Run();

    }
}


