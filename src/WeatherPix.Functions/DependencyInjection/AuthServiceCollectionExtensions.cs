using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Extensions.Configuration;
using WeatherPix.Functions.Helpers.Auth;
using WeatherPix.Functions.Helpers.Auth.Options;

namespace WeatherPix.Functions.DependencyInjection;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(
        this IServiceCollection services,
        IConfiguration cfg)
    {
        services
            .AddOptions<AuthOptions>()
            .Bind(cfg.GetSection(AuthOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<AuthOptions>>()
                .Value;

            return new ConfigurationManager<OpenIdConnectConfiguration>(
                $"https://{options.Domain}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());
        });

        services.AddSingleton<IAuthService, AuthService>();

        return services;
    }
}