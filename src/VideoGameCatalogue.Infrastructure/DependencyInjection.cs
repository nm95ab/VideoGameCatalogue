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
