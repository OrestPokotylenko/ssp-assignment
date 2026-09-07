using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Abstractions;

public interface IQueueJobHandler
{
    Task<Result<Guid>> QueueJobAsync(CancellationToken ct);
}