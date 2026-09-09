using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using WeatherPix.Application.Jobs.GetStatus;
using WeatherPix.Functions.Helpers;
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
        Route = "jobs/{operationId}/status")]
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


        var result = await _handler.HandleAsync(
            parsed.OperationId,
            ct);

        if (result.IsFailure)
        {
            var response = req.CreateResponse(
                HttpStatusCode.InternalServerError);

            return response;
        }

        var data = result.Value;

        if (data is null)
        {
            return req.CreateResponse(
                HttpStatusCode.NotFound);
        }

        var okResponse = req.CreateResponse(
            HttpStatusCode.OK);

        await okResponse.WriteAsJsonAsync(
            new JobStatusResponse(
                data.Job.OperationId,
                data.Job.Status,
                data.Job.CreatedAt,
                data.Progress),
            ct);

        return okResponse;
    }
}