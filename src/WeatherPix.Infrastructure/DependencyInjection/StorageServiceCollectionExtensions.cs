using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Infrastructure.Options.Table;
using WeatherPix.Infrastructure.Storage;

namespace WeatherPix.Infrastructure.DependencyInjection;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddStorage(
        this IServiceCollection services,
        IConfiguration cfg)
    {
        services
            .AddOptions<StorageOptions>()
            .Bind(cfg.GetSection(StorageOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton(sp =>
        {
            var options = sp
            .GetRequiredService<IOptions<StorageOptions>>()
            .Value;

            return new TableServiceClient(
                new Uri(options.TableServiceUri),
                new DefaultAzureCredential());
        });

        services.AddSingleton<IJobStatusRepository, JobStatusRepository>();

        return services;
    }
}