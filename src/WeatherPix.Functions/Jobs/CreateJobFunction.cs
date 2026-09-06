using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using WeatherPix.Application.Abstractions;
using WeatherPix.Functions.Jobs.Contracts;

namespace WeatherPix.Functions.Jobs;

public class CreateJobFunction(IQueueJobHandler queueJobHandler)
{
    private readonly IQueueJobHandler _queueJobHandler = queueJobHandler;


    [Function("CreateJobFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "jobs")]
        HttpRequestData req, CancellationToken ct)
    {
        var result = await _queueJobHandler.QueueJobAsync(ct);

        if (!result.IsSuccess)
        {

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(
                new ErrorResponse(
                    result.Error!.Code,
                    result.Error.Message
                ),
                ct
            );

            return errorResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.Accepted);
        await response.WriteAsJsonAsync(
            new QueuedJobResponse(result.Value),
            ct
        );

        return response;
    }
}