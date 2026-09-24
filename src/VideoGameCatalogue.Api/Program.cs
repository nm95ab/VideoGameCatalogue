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

// Centralized RFC 7807 exception handling middleware
app.UseExceptionHandler();

// Configure the HTTP request pipeline
app.UseCors(angularCorsPolicy);

// High-performance static file serving for local image storage
var uploadsDir = Path.Combine(app.Environment.ContentRootPath, "uploads", "images");
if (!Directory.Exists(uploadsDir))
{
    Directory.CreateDirectory(uploadsDir);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsDir),
    RequestPath = "/api/images",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "public, max-age=86400";
    }
});

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
    await DatabaseSeeder.MigrateSchemaAsync(context);
}

// Map Endpoints
app.MapVideoGameEndpoints();
app.MapImageEndpoints();

app.Run();

// Required for integration testing WebApplicationFactory
public partial class Program { }
