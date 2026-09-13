using WeatherPix.Application.Messaging;

namespace WeatherPix.Application.Abstractions;

public interface IMessagePublisher
{
    Task PublishAsync<T>(
        T payload,
        CancellationToken ct)
        where T : IMessage;

    Task PublishBatchAsync<T>(
        IReadOnlyCollection<T> payloads,
        CancellationToken ct)
        where T : IMessage;
}