using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoGameCatalogue.Domain.Ports;
using VideoGameCatalogue.Infrastructure.Persistence;
using VideoGameCatalogue.Infrastructure.Persistence.Repositories;

namespace VideoGameCatalogue.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var useInMemory = configuration.GetValue("UseInMemoryDatabase", false);

        var shouldUseInMemory = useInMemory ||
                                string.IsNullOrWhiteSpace(connectionString) ||
                                (connectionString?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) ?? false);

        if (shouldUseInMemory)
        {
            services.AddDbContext<VideoGameCatalogueDbContext>(options =>
                options.UseInMemoryDatabase("VideoGameCatalogueDb"));
        }
        else
        {
            services.AddDbContext<VideoGameCatalogueDbContext>(options =>
                options.UseSqlServer(connectionString!, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));
        }

        services.AddScoped<IVideoGameRepository, EfCoreVideoGameRepository>();
        services.AddScoped<ILookupRepository, EfCoreLookupRepository>();

        return services;
    }
}
