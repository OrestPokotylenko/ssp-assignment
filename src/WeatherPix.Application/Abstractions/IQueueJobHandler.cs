using WeatherPix.Application.Common.Results;
using WeatherPix.Domain.Jobs;

namespace WeatherPix.Application.Abstractions;

public interface IQueueJobHandler
{
    Task<Result<Guid>> QueueJobAsync(CancellationToken ct);
}