using Azure.Data.Tables;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Infrastructure.Options.Storage;
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
            .ValidateDataAnnotations()
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

        services.AddSingleton(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<StorageOptions>>()
                .Value;

            return new BlobServiceClient(
                new Uri(options.BlobServiceUri),
                new DefaultAzureCredential());
        });

        services.AddSingleton<IImageStorage, BlobImageStorage>();

        return services;
    }
}