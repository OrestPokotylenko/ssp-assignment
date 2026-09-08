using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using WeatherPix.Application.Abstractions;
using WeatherPix.Infrastructure.Images;
using WeatherPix.Infrastructure.Options.Pexels;

namespace WeatherPix.Infrastructure.DependencyInjection;

public static class ImagesServiceCollectionExtensions
{
    public static IServiceCollection AddPexels(
        this IServiceCollection services,
        IConfiguration cfg)
    {
        services.AddOptions<PexelsOptions>()
            .Bind(cfg.GetSection(PexelsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();


        services.AddHttpClient<IImageProvider, PexelsImageProvider>(
            (sp, client) =>
            {
                var options = sp
                    .GetRequiredService<IOptions<PexelsOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(options.ApiKey);
            });

        services.AddSingleton<IWeatherImageRenderer, ImageSharpWeatherImageRenderer>();

        return services;
    }
}