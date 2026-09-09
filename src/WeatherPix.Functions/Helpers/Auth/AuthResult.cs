using System.Net;

namespace WeatherPix.Functions.Helpers.Auth;

public sealed record AuthResult(
    bool IsAuthorized,
    HttpStatusCode StatusCode,
    string? Error)
{
    public static AuthResult Success() =>
        new(
            true,
            HttpStatusCode.OK,
            null);

    public static AuthResult Unauthorized() =>
        new(
            false,
            HttpStatusCode.Unauthorized,
            "Invalid or missing access token.");

    public static AuthResult Forbidden() =>
        new(
            false,
            HttpStatusCode.Forbidden,
            "Insufficient permissions.");
}