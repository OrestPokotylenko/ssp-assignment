using WeatherPix.Domain.Jobs;

namespace WeatherPix.Application.Abstractions;

public interface IJobQueuePublisher
{
    Task PublishJobAsync(Job job, CancellationToken ct);
}