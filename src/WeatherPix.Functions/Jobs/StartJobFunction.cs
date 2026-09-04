using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace WeatherPix.Functions.Functions;

public class StartJobFunction
{
    private readonly ILogger<StartJobFunction> _logger;

    public StartJobFunction(ILogger<StartJobFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(StartJobFunction))]
    public async Task Run(
        [ServiceBusTrigger("myqueue", Connection = "vbsacbkc")]
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