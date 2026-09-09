using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using WeatherPix.Application.Jobs.GetResults;

namespace WeatherPix.Functions.Functions;

public class GetJobResultsFunction(GetJobResultsHandler handler)
{
    private readonly GetJobResultsHandler _handler = handler;

    [Function("GetJobResultsFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "jobs/{operationId:guid}/results")]
        HttpRequestData req,
        Guid operationId,
        CancellationToken ct)
    {
        var result = await _handler.HandleAsync(operationId, ct);

        if (result.IsFailure)
        {
            var response =
                req.CreateResponse(HttpStatusCode.InternalServerError);

            await response.WriteAsJsonAsync(
                new { error = result.Error },
                ct);

            return response;
        }

        if (result.Value is null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var success =
            req.CreateResponse(HttpStatusCode.OK);

        await success.WriteAsJsonAsync(result.Value, ct);

        return success;
    }
}