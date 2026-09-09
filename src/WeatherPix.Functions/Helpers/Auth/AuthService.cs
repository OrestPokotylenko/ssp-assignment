using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using WeatherPix.Functions.Helpers.Auth.Options;
using FunctionHttpRequestData = Microsoft.Azure.Functions.Worker.Http.HttpRequestData;

namespace WeatherPix.Functions.Helpers.Auth;

public sealed class AuthService(
    IOptions<AuthOptions> options,
    IConfigurationManager<OpenIdConnectConfiguration> configurationManager)
    : IAuthService
{
    private const string BearerPrefix = "Bearer ";

    private readonly AuthOptions _options = options.Value;

    private readonly IConfigurationManager<OpenIdConnectConfiguration>
        _configurationManager = configurationManager;

    public async Task<AuthResult> AuthorizeAsync(
        FunctionHttpRequestData request,
        string requiredScope,
        CancellationToken ct)
    {
        if (!request.Headers.TryGetValues(
                "Authorization",
                out var values))
        {
            return AuthResult.Unauthorized();
        }

        var header = values.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(header) ||
            !header.StartsWith(
                BearerPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return AuthResult.Unauthorized();
        }

        var token = header[BearerPrefix.Length..].Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthResult.Unauthorized();
        }

        try
        {
            var config =
                await _configurationManager.GetConfigurationAsync(ct);

            var validationParameters =
                new TokenValidationParameters
                {
                    ValidIssuer = $"https://{_options.Domain}/",
                    ValidAudience = _options.Audience,

                    IssuerSigningKeys = config.SigningKeys,

                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };

            var tokenHandler =
                new JwtSecurityTokenHandler();

            var principal =
                tokenHandler.ValidateToken(
                    token,
                    validationParameters,
                    out _);

            var scopes =
                principal.FindFirst("scope")?.Value
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries)
                ?? [];

            if (!scopes.Contains(
                    requiredScope,
                    StringComparer.Ordinal))
            {
                return AuthResult.Forbidden();
            }

            return AuthResult.Success();
        }
        catch (SecurityTokenException)
        {
            return AuthResult.Unauthorized();
        }
        catch (ArgumentException)
        {
            return AuthResult.Unauthorized();
        }
    }
}