using Microsoft.Extensions.DependencyInjection;
using VideoGameCatalogue.Application.Games;

namespace VideoGameCatalogue.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IVideoGameService, VideoGameService>();
        services.AddScoped<Images.IImageManagementService, Images.ImageManagementService>();
        return services;
    }
}
