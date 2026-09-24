using Microsoft.Extensions.DependencyInjection;
using VideoGameCatalogue.Application.Games;

namespace VideoGameCatalogue.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IVideoGameService, VideoGameService>();
        return services;
    }
}
