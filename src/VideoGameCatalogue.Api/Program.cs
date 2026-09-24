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
    await context.Database.EnsureCreatedAsync();
    await DatabaseSeeder.SeedAsync(context);
}

// Map Endpoints
app.MapVideoGameEndpoints();

app.Run();

// Required for integration testing WebApplicationFactory
public partial class Program { }
