using Microsoft.Extensions.DependencyInjection;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Jobs;

namespace WeatherPix.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IQueueJobHandler, QueueJobHandler>();

        return services;
    }
}