using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        var token = GetBearerToken(request);

        if (token is null)
        {
            return AuthResult.Unauthorized();
        }

        try
        {
            var principal = await ValidateTokenAsync(token, ct);

            var subject = principal.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(subject))
            {
                return AuthResult.Unauthorized();
            }

            if (!HasScope(principal, requiredScope))
            {
                return AuthResult.Forbidden();
            }

            return AuthResult.Success(subject);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
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

    private static string? GetBearerToken(
        FunctionHttpRequestData request)
    {
        if (!request.Headers.TryGetValues(
                "Authorization",
                out var values))
        {
            return null;
        }

        var header = values.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(header) ||
            !header.StartsWith(
                BearerPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = header[BearerPrefix.Length..].Trim();

        return string.IsNullOrWhiteSpace(token)
            ? null
            : token;
    }

    private async Task<ClaimsPrincipal> ValidateTokenAsync(
        string token,
        CancellationToken ct)
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
            new JwtSecurityTokenHandler
            {
                MapInboundClaims = false
            };

        return tokenHandler.ValidateToken(
            token,
            validationParameters,
            out _);
    }

    private static bool HasScope(
        ClaimsPrincipal principal,
        string requiredScope)
    {
        var scopes =
            principal.FindFirst("scope")?.Value
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries)
            ?? [];

        return scopes.Contains(
            requiredScope,
            StringComparer.Ordinal);
    }
}