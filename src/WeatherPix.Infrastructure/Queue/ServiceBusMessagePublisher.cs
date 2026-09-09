using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Messaging;
using WeatherPix.Infrastructure.Options.ServiceBus;

namespace WeatherPix.Infrastructure.Queue;

public class ServiceBusMessagePublisher(
    ServiceBusClient client,
    IOptions<ServiceBusOptions> options)
    : IMessagePublisher
{
    private readonly ServiceBusSender _startJobSender =
    client.CreateSender(options.Value.StartJobsQueueName);

    private readonly ServiceBusSender _imageJobSender =
        client.CreateSender(options.Value.ImageJobsQueueName);

    public async Task PublishAsync<T>(
        T payload,
        CancellationToken ct) where T : IMessage
    {
        var sender = GetSenderForType<T>();

        var message = CreateMessage(payload);

        await sender.SendMessageAsync(message, ct);
    }

    public async Task PublishBatchAsync<T>(
        IReadOnlyCollection<T> payloads,
        CancellationToken ct)
        where T : IMessage
    {
        if (payloads.Count == 0)
        {
            return;
        }

        var sender = GetSenderForType<T>();

        ServiceBusMessageBatch? batch = null;

        try
        {
            batch = await sender.CreateMessageBatchAsync(ct);

            foreach (var payload in payloads)
            {
                var message = CreateMessage(payload);

                if (batch.TryAddMessage(message))
                {
                    continue;
                }

                await sender.SendMessagesAsync(batch, ct);

                batch.Dispose();
                batch = await sender.CreateMessageBatchAsync(ct);

                if (!batch.TryAddMessage(message))
                {
                    throw new InvalidOperationException(
                        $"Message of type {typeof(T).Name} is too large for a Service Bus batch.");
                }
            }

            if (batch.Count > 0)
            {
                await sender.SendMessagesAsync(batch, ct);
            }
        }
        finally
        {
            batch?.Dispose();
        }
    }

    private static ServiceBusMessage CreateMessage<T>(
        T payload)
        where T : IMessage
    {
        return new ServiceBusMessage(
            BinaryData.FromObjectAsJson(payload))
        {
            ContentType = "application/json"
        };
    }

    private ServiceBusSender GetSenderForType<T>()
    {
        return typeof(T) switch
        {
            var type when type == typeof(StartJobMessage) =>
                _startJobSender,

            var type when type == typeof(GenerateImageMessage) =>
                _imageJobSender,

            _ => throw new InvalidOperationException(
                $"No queue configured for message type {typeof(T).Name}.")
        };
    }
}