using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Api.Endpoints;
using VideoGameCatalogue.Application;
using VideoGameCatalogue.Infrastructure;
using VideoGameCatalogue.Infrastructure.Persistence;

const string angularCorsPolicy = "AllowAngularClient";
const string uploadsRateLimitPolicy = "UploadsPolicy";

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200", "http://127.0.0.1:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(angularCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

ConfigureRateLimiting(builder.Services, builder.Configuration);

var app = builder.Build();

// 1. Resolve true client IP and proto when hosted behind reverse proxies / load balancers
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// 2. Centralized RFC 7807 exception handling middleware
app.UseExceptionHandler();

// 3. Defensive HTTP security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// 4. Configure CORS
app.UseCors(angularCorsPolicy);

// 5. High-performance static file serving for local image storage (served before rate limiter to prevent image grids from depleting API quota)
var uploadsDir = Path.Combine(app.Environment.ContentRootPath, "uploads", "images");
if (!Directory.Exists(uploadsDir))
{
    Directory.CreateDirectory(uploadsDir);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsDir),
    RequestPath = "/api/images",
    ContentTypeProvider = CreateRestrictedImageContentTypeProvider(),
    ServeUnknownFileTypes = false,
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
    }
});

app.UseRouting();

// 6. Dynamic API rate limiting (applies only to dynamic API endpoints)
app.UseRateLimiter();

// Initialize & Seed Database (Code First)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VideoGameCatalogueDbContext>();
    await EnsureDatabaseInitializedAsync(context);

    if (app.Environment.IsDevelopment() || builder.Configuration.GetValue("Database:SeedSampleData", false))
    {
        await DatabaseSeeder.SeedAsync(context);
    }
}

static async Task EnsureDatabaseInitializedAsync(VideoGameCatalogueDbContext context)
{
    await context.Database.EnsureCreatedAsync();
    await DatabaseSeeder.MigrateSchemaAsync(context);
}

static void ConfigureRateLimiting(IServiceCollection services, IConfiguration configuration)
{
    var generalLimit = configuration.GetValue("RateLimiting:GeneralPermitLimit", 150);
    var uploadsLimit = configuration.GetValue("RateLimiting:UploadsPermitLimit", 30);

    services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            if (HttpMethods.IsGet(httpContext.Request.Method) &&
                httpContext.Request.Path.StartsWithSegments("/api/images"))
            {
                return RateLimitPartition.GetNoLimiter("images-unrestricted");
            }

            var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: clientIp,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = generalLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        });

        options.AddPolicy(uploadsRateLimitPolicy, httpContext =>
        {
            var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: clientIp,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = uploadsLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
        });
    });
}

static FileExtensionContentTypeProvider CreateRestrictedImageContentTypeProvider()
{
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings.Clear();
    provider.Mappings[".webp"] = "image/webp";
    provider.Mappings[".png"] = "image/png";
    provider.Mappings[".jpg"] = "image/jpeg";
    provider.Mappings[".jpeg"] = "image/jpeg";
    return provider;
}

// Map Endpoints
app.MapVideoGameEndpoints();
app.MapImageEndpoints();

app.Run();

// Required for integration testing WebApplicationFactory
public partial class Program { }
