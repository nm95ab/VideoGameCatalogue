using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Api.Endpoints;
using VideoGameCatalogue.Application;
using VideoGameCatalogue.Infrastructure;
using VideoGameCatalogue.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();

const string angularCorsPolicy = "AllowAngularClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(angularCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors(angularCorsPolicy);

// Initialize & Seed Database (Code First)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VideoGameCatalogueDbContext>();
    await EnsureDatabaseInitializedAsync(context);
    await DatabaseSeeder.SeedAsync(context);
}

static async Task EnsureDatabaseInitializedAsync(VideoGameCatalogueDbContext context)
{
    await context.Database.EnsureCreatedAsync();
    if (!context.Database.IsRelational())
        return;

    try
    {
        await context.Database.ExecuteSqlRawAsync("SELECT TOP 1 1 FROM Platforms");
    }
    catch
    {
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}

// Map Endpoints
app.MapVideoGameEndpoints();

app.Run();

// Required for integration testing WebApplicationFactory
public partial class Program { }
