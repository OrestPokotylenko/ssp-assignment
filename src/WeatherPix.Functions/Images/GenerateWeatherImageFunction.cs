using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WeatherPix.Application.Images.Handler;
using WeatherPix.Application.Messaging;

namespace WeatherPix.Functions.Images;

public class GenerateWeatherImageFunction(
    IGenerateWeatherImageHandler handler,
    ILogger<GenerateWeatherImageFunction> logger)
{
    private readonly IGenerateWeatherImageHandler _handler = handler;
    private readonly ILogger<GenerateWeatherImageFunction> _logger = logger;

    [Function(nameof(GenerateWeatherImageFunction))]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBus:ImageJobsQueueName%", Connection = "ServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken ct)
    {
        GenerateImageMessage? payload;

        try
        {
            payload =
                message.Body.ToObjectFromJson<GenerateImageMessage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Invalid GenerateImageMessage");

            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "InvalidMessage",
                deadLetterErrorDescription:
                    "Failed to deserialize image job.",
                cancellationToken: ct);

            return;
        }

        if (payload is null)
        {
            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "InvalidMessage",
                deadLetterErrorDescription:
                    "Message body was empty.",
                cancellationToken: ct);

            return;
        }

        var result = await _handler.HandleAsync(
            payload,
            ct);

        if (result.IsSuccess)
        {
            await messageActions.CompleteMessageAsync(
                message,
                ct);

            return;
        }

        await messageActions.DeadLetterMessageAsync(
            message,
            deadLetterReason: result.Error!.Code,
            deadLetterErrorDescription: result.Error.Message,
            cancellationToken: ct);
    }
}