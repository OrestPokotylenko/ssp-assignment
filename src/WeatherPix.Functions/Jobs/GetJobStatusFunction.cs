using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using System.Net;
using WeatherPix.Application.Jobs.GetStatus;
using WeatherPix.Functions.Helpers.Auth;
using WeatherPix.Functions.Helpers.Auth.Options;
using WeatherPix.Functions.Helpers.OperationId;
using WeatherPix.Functions.Jobs.Contracts;

namespace WeatherPix.Functions.Jobs;

public class GetJobStatusFunction(
    IGetJobStatusHandler handler,
    IAuthService authService,
    IOptions<AuthOptions> options)
{
    private readonly IGetJobStatusHandler _handler = handler;
    private readonly IAuthService _authService = authService;
    private readonly AuthOptions _options = options.Value;

    [Function("GetJobStatusFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous,
        "get",
        Route = "jobs/{operationId}/status")]
        HttpRequestData req,
        string operationId,
        CancellationToken ct)
    {
        try
        {
            var auth = await _authService.AuthorizeAsync(
                req,
                _options.ReadPermission,
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