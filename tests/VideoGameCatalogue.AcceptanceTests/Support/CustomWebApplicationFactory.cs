using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoGameCatalogue.Infrastructure.Persistence;

namespace VideoGameCatalogue.AcceptanceTests.Support;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AcceptanceConnectionString =
        "Server=127.0.0.1,1433;Database=VideoGameCatalogueAcceptanceDb;User Id=sa;Password=YourStrong@Password123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connect Timeout=15";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = AcceptanceConnectionString,
                ["UseInMemoryDatabase"] = "false"
            });
        });
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VideoGameCatalogueDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
        await context.VideoGames.ExecuteDeleteAsync(cancellationToken);
        await DatabaseSeeder.SeedAsync(context, cancellationToken);
    }
}
