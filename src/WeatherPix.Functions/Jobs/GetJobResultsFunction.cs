using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using WeatherPix.Application.Jobs.GetResults;
using WeatherPix.Functions.Helpers;
using WeatherPix.Functions.Jobs.Contracts;

namespace WeatherPix.Functions.Functions;

public class GetJobResultsFunction(GetJobResultsHandler handler)
{
    private readonly GetJobResultsHandler _handler = handler;

    [Function("GetJobResultsFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "jobs/{operationId}/results")]
        HttpRequestData req,
        string operationId,
        CancellationToken ct)
    {
        var parsed = await OperationIdRequestParser.ParseAsync(
            operationId,
            req,
            ct);


        if (!parsed.Success && parsed.ErrorResponse is not null)
        {
            return parsed.ErrorResponse;
        }

        var result = await _handler.HandleAsync(parsed.OperationId, ct);

        if (result.IsFailure)
        {
            var response =
                req.CreateResponse(HttpStatusCode.InternalServerError);

            await response.WriteAsJsonAsync(
                new { error = result.Error },
                ct);

            return response;
        }

        var data = result.Value;

        if (data is null)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var success =
            req.CreateResponse(HttpStatusCode.OK);

        await success.WriteAsJsonAsync(new JobResultsResponse(
            data.OperationId,
            data.Status,
            data.Images
            ),
            ct);

        return success;
    }
}