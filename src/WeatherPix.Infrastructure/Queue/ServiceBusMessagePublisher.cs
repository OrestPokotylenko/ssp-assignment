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
    private readonly ServiceBusClient _client = client;
    private readonly ServiceBusOptions _options = options.Value;

    public async Task PublishAsync<T>(
        T payload,
        CancellationToken ct) where T : IMessage
    {
        var queueName = GetQueueNameForType<T>();

        var sender = _client.CreateSender(queueName);

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

        var queueName = GetQueueNameForType<T>();

        await using var sender = _client.CreateSender(queueName);

        ServiceBusMessageBatch batch =
            await sender.CreateMessageBatchAsync(ct);

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

        batch.Dispose();
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

    private string GetQueueNameForType<T>()
    {
        return typeof(T) switch
        {
            var type when type == typeof(StartJobMessage) =>
                _options.StartJobsQueueName,

            var type when type == typeof(GenerateImageMessage) =>
                _options.ImageJobsQueueName,

            _ => throw new InvalidOperationException(
                $"No queue configured for message type {typeof(T).Name}.")
        };
    }
}