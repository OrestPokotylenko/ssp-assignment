using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace WeatherPix.Functions.Functions;

public class GetJobStatusFunction
{
    private readonly ILogger<GetJobStatusFunction> _logger;

    public GetJobStatusFunction(ILogger<GetJobStatusFunction> logger)
    {
        _logger = logger;
    }

    [Function("GetJobStatusFunction")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}