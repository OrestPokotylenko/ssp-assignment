using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace WeatherPix.Functions.Helpers;

public static class OperationIdRequestParser
{

    public static async Task<(bool Success, Guid OperationId, HttpResponseData? ErrorResponse)> ParseAsync(
        string operationId,
        HttpRequestData req,
        CancellationToken ct)
    {
        if (Guid.TryParse(operationId, out var parsedOperationId))
        {
            return (true, parsedOperationId, null);
        }

        var badRequest =
            req.CreateResponse(HttpStatusCode.BadRequest);

        await badRequest.WriteAsJsonAsync(
            new
            {
                error = "Invalid operationId. Expected a valid GUID."
            },
            ct);

        return (false, Guid.Empty, badRequest);
    }
}