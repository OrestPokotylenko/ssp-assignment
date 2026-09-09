using Azure.Core;
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
            var credential = sp.GetRequiredService<TokenCredential>();

            var options = sp
            .GetRequiredService<IOptions<StorageOptions>>()
            .Value;

            var clientOptions = new TableClientOptions
            {
                Retry =
                {
                    Mode = RetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(5),
                    NetworkTimeout = TimeSpan.FromSeconds(10)
                }
            };

            return new TableServiceClient(
                new Uri(options.TableServiceUri),
                credential,
                clientOptions);
        });

        services.AddSingleton<IJobStatusRepository, JobStatusRepository>();

        services.AddSingleton(sp =>
        {
            var credential = sp.GetRequiredService<TokenCredential>();

            var options = sp
                .GetRequiredService<IOptions<StorageOptions>>()
                .Value;

            var blobOptions = new BlobClientOptions
            {
                Retry =
                {
                    Mode = RetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(5),
                    NetworkTimeout = TimeSpan.FromSeconds(15)
                }
            };

            return new BlobServiceClient(
                new Uri(options.BlobServiceUri),
                credential,
                blobOptions);
        });

        services.AddSingleton<IImageStorage, BlobImageStorage>();

        return services;
    }
}