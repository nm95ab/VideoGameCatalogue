using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoGameCatalogue.Application.Common.Ports;
using VideoGameCatalogue.Domain.Ports;
using VideoGameCatalogue.Infrastructure.Images;
using VideoGameCatalogue.Infrastructure.Persistence;
using VideoGameCatalogue.Infrastructure.Persistence.Repositories;
using VideoGameCatalogue.Infrastructure.Storage;

namespace VideoGameCatalogue.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var useInMemory = configuration.GetValue("UseInMemoryDatabase", false);

        var shouldUseInMemory = useInMemory ||
                                (connectionString?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) ?? false);

        if (shouldUseInMemory)
        {
            services.AddDbContext<VideoGameCatalogueDbContext>(options =>
                options.UseInMemoryDatabase("VideoGameCatalogueDb"));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' was not found. " +
                    "For local development, verify that 'appsettings.Development.json' is present and ASPNETCORE_ENVIRONMENT is set to 'Development'. " +
                    "In production, provide the connection string via the 'ConnectionStrings__DefaultConnection' environment variable or secret store.");
            }

            services.AddDbContext<VideoGameCatalogueDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));
        }

        services.AddMemoryCache();
        services.AddScoped<IVideoGameRepository, EfCoreVideoGameRepository>();
        services.AddScoped<EfCoreLookupRepository>();
        services.AddScoped<ILookupRepository>(sp =>
            new CachedLookupRepository(
                sp.GetRequiredService<EfCoreLookupRepository>(),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>()));

        services.AddSingleton<IImageThumbnailProcessor, ImageSharpThumbnailProcessor>();
        RegisterImageStorage(services, configuration);

        return services;
    }

    private static void RegisterImageStorage(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["ImageStorage:Provider"];
        if (string.Equals(provider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            var azureConnectionString = configuration["ImageStorage:AzureBlob:ConnectionString"] ?? string.Empty;
            var containerName = configuration["ImageStorage:AzureBlob:ContainerName"] ?? "game-thumbnails";
            var baseUrl = configuration["ImageStorage:AzureBlob:BaseUrl"] ?? configuration["ImageStorage:BaseUrl"];
            services.AddScoped<IImageStoragePort>(_ => new AzureBlobStorageImageStorageAdapter(azureConnectionString, containerName, baseUrl));
        }
        else
        {
            var localPath = configuration["ImageStorage:LocalStoragePath"];
            var localBaseUrl = configuration["ImageStorage:LocalBaseUrl"] ?? configuration["ImageStorage:BaseUrl"];
            services.AddScoped<IImageStoragePort>(_ => new LocalStorageImageStorageAdapter(localPath, localBaseUrl));
        }
    }
}
