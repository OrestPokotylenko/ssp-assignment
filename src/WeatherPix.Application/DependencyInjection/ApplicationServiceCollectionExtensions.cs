using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Images;
using WeatherPix.Application.Jobs.GetResults;
using WeatherPix.Application.Jobs.GetStatus;
using WeatherPix.Application.Jobs.QueueJob;
using WeatherPix.Application.Jobs.StartJob;
using WeatherPix.Application.Options.WeatherStation;

namespace WeatherPix.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration cfg)
    {
        services.AddOptions<WeatherStationOptions>()
            .Bind(cfg.GetSection(WeatherStationOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IQueueJobHandler, QueueJobHandler>();
        services.AddScoped<IStartJobHandler, StartJobHandler>();
        services.AddScoped<IGetJobStatusHandler, GetJobStatusHandler>();
        services.AddScoped<IGenerateWeatherImageHandler, GenerateWeatherImageHandler>();
        services.AddScoped<GetJobResultsHandler>();

        return services;
    }
}