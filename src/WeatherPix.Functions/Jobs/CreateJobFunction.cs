using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace WeatherPix.Functions.Functions;

public class CreateJobFunction
{
    private readonly ILogger<CreateJobFunction> _logger;

    public CreateJobFunction(ILogger<CreateJobFunction> logger)
    {
        _logger = logger;
    }

    [Function("CreateJobFunction")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}