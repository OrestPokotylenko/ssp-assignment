using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WeatherPix.Infrastructure.DependencyInjection;

public static class AzureCredentialServiceCollectionExtensions
{
    public static IServiceCollection AddAzureCredentials(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddSingleton<TokenCredential>(
                new AzureCliCredential());
        }
        else
        {
            services.AddSingleton<TokenCredential>(
                new DefaultAzureCredential());
        }

        return services;
    }
}