using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using WeatherPix.Application.Jobs.GetStatus;
using WeatherPix.Functions.Jobs.Contracts;

namespace WeatherPix.Functions.Jobs;

public class GetJobStatusFunction(
    IGetJobStatusHandler handler,
    ILogger<GetJobStatusFunction> logger)
{
    private readonly IGetJobStatusHandler _handler = handler;
    private readonly ILogger<GetJobStatusFunction> _logger = logger;


    [Function("GetJobStatusFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous,
        "get",
        Route = "jobs/{operationId:guid}/status")]
        HttpRequestData req,
        Guid operationId,
        CancellationToken ct)
    {
        var result = await _handler.HandleAsync(
            operationId,
            ct);

        if (result.IsFailure)
        {
            var response = req.CreateResponse(
                HttpStatusCode.InternalServerError);

            return response;
        }

        if (result.Value is null)
        {
            return req.CreateResponse(
                HttpStatusCode.NotFound);
        }

        var okResponse = req.CreateResponse(
            HttpStatusCode.OK);

        await okResponse.WriteAsJsonAsync(
            new JobStatusResponse(
                result.Value.OperationId,
                result.Value.Status,
                result.Value.CreatedAt),
            ct);

        return okResponse;
    }
}