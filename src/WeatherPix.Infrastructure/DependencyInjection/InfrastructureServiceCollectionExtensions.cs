using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WeatherPix.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration cfg)
    {
        services.AddServiceBus(cfg);
        services.AddStorage(cfg);
        services.AddBuienradar(cfg);
        services.AddPexels(cfg);

        return services;
    }
}