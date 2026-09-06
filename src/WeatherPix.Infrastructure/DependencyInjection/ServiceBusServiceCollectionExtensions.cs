using Azure.Identity;
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
            .ValidateOnStart();

        services.AddSingleton(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<ServiceBusOptions>>()
                .Value;

            return new ServiceBusClient(
                options.FullyQualifiedNamespace, 
                new DefaultAzureCredential());

        });

        services.AddSingleton<IJobQueuePublisher, JobQueuePublisher>();

        return services;
    }
}