using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace WeatherPix.Functions.Images;

public class GenerateWeatherImageFunction
{
    private readonly ILogger<GenerateWeatherImageFunction> _logger;

    public GenerateWeatherImageFunction(ILogger<GenerateWeatherImageFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(GenerateWeatherImageFunction))]
    public async Task Run(
        [ServiceBusTrigger("myqueue", Connection = "accac")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        // Complete the message
        await messageActions.CompleteMessageAsync(message);
    }
}