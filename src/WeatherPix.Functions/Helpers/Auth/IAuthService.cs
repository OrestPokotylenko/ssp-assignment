using Microsoft.Azure.Functions.Worker.Http;

namespace WeatherPix.Functions.Helpers.Auth;

public interface IAuthService
{
    Task<AuthResult> AuthorizeAsync(
        HttpRequestData request,
        string requiredScope,
        CancellationToken ct);
}