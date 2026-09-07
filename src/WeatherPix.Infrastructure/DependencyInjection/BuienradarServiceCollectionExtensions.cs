using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Infrastructure.Options.Buienradar;
using WeatherPix.Infrastructure.Weather;

namespace WeatherPix.Infrastructure.DependencyInjection;

public static class BuienradarServiceCollectionExtensions
{
    public static IServiceCollection AddBuienradar(this IServiceCollection services, IConfiguration cfg)
    {
        services
            .AddOptions<BuienradarOptions>()
            .Bind(cfg.GetSection(BuienradarOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IWeatherStationProvider, BuienradarWeatherStationProvider>((sp, client) =>
        {
            var options =
                sp.GetRequiredService<IOptions<BuienradarOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}