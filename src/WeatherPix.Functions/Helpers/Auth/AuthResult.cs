using System.Net;

namespace WeatherPix.Functions.Helpers.Auth;

public sealed record AuthResult(
    bool IsAuthorized,
    HttpStatusCode StatusCode,
    string? Subject,
    string? Error)
{
    public static AuthResult Success(string subject) =>
        new(
            true,
            HttpStatusCode.OK,
            subject,
            null);

    public static AuthResult Unauthorized() =>
        new(
            false,
            HttpStatusCode.Unauthorized,
            null,
            "Invalid or missing access token.");

    public static AuthResult Forbidden() =>
        new(
            false,
            HttpStatusCode.Forbidden,
            null,
            "Insufficient permissions.");
}