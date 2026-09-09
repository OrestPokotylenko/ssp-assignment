using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using System.Net;
using WeatherPix.Application.Abstractions;
using WeatherPix.Functions.Helpers.Auth;
using WeatherPix.Functions.Helpers.Auth.Options;
using WeatherPix.Functions.Jobs.Contracts;

namespace WeatherPix.Functions.Jobs;

public class CreateJobFunction(
    IQueueJobHandler queueJobHandler,
    IAuthService authService,
    IOptions<AuthOptions> options)
{
    private readonly IQueueJobHandler _queueJobHandler = queueJobHandler;
    private readonly IAuthService _authService = authService;
    private readonly AuthOptions _options = options.Value;

    [Function("CreateJobFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous,
        "post",
        Route = "jobs")]
        HttpRequestData req,
        CancellationToken ct)
    {
        try
        {
            var auth = await _authService.AuthorizeAsync(
            req,
            _options.CreatePermission,
            ct);

            if (!auth.IsAuthorized)
            {
                var unauthorizedResponse = req.CreateResponse(auth.StatusCode);

                await unauthorizedResponse.WriteAsJsonAsync(
                    new
                    {
                        error = auth.Error
                    },
                    ct);

                return unauthorizedResponse;
            }

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
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(
                new ErrorResponse(
                    "InternalServerError",
                    ex.Message
                ),
                ct
            );
            return errorResponse;
        }
    }
}