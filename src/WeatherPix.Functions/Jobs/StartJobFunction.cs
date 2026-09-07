using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WeatherPix.Application.Jobs.StartJob;
using WeatherPix.Application.Messaging;

namespace WeatherPix.Functions.Jobs;

public class StartJobFunction(
    IStartJobHandler startJobHandler,
    ILogger<StartJobFunction> logger)
{
    private readonly IStartJobHandler _startJobHandler = startJobHandler;
    private readonly ILogger<StartJobFunction> _logger = logger;


    [Function(nameof(StartJobFunction))]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBus:StartJobsQueueName%", Connection = "ServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken ct)
    {
        StartJobMessage? payload;

        try
        {
            payload = message.Body.ToObjectFromJson<StartJobMessage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize StartJobMessage. MessageId: {MessageId}",
                message.MessageId);

            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "InvalidMessage",
                deadLetterErrorDescription: "Failed to deserialize message.",
                cancellationToken: ct);

            return;
        }

        if (payload is null)
        {
            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "InvalidMessage",
                deadLetterErrorDescription: "Message body was empty.",
                cancellationToken: ct);

            return;
        }

        var result = await _startJobHandler.HandleAsync(
            payload.OperationId,
            ct);

        if (result.IsSuccess)
        {
            await messageActions.CompleteMessageAsync(
                message,
                ct);

            return;
        }

        _logger.LogWarning(
            "Start job failed for operation {OperationId}. Error: {ErrorCode}",
            payload.OperationId,
            result.Error?.Code);

        await messageActions.DeadLetterMessageAsync(
            message,
            deadLetterReason: result.Error?.Code ?? "StartJobFailed",
            deadLetterErrorDescription:
                result.Error?.Message ?? "Failed to start job.",
            cancellationToken: ct);
    }
}