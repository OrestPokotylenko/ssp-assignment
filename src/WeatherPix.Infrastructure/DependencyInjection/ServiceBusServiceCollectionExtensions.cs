using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Infrastructure.Options.ServiceBus;
using WeatherPix.Infrastructure.Queue;

namespace WeatherPix.Infrastructure.DependencyInjection;

public static class ServiceBusServiceCollectionExtensions
{
    public static IServiceCollection AddServiceBus(
        this IServiceCollection services,
        IConfiguration cfg)
    {
        services
            .AddOptions<ServiceBusOptions>()
            .Bind(cfg.GetSection(ServiceBusOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(sp =>
        {
            var credential = sp.GetRequiredService<TokenCredential>();

            var options = sp
                .GetRequiredService<IOptions<ServiceBusOptions>>()
                .Value;

            var serviceBusOptions = new ServiceBusClientOptions
            {
                RetryOptions =
                {
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(5),
                    TryTimeout = TimeSpan.FromSeconds(10)
                }
            };

            return new ServiceBusClient(
                options.FullyQualifiedNamespace,
                credential,
                serviceBusOptions);

        });

        services.AddSingleton<IMessagePublisher, ServiceBusMessagePublisher>();

        return services;
    }
}