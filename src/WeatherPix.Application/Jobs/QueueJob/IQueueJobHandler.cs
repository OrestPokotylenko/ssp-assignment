using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.QueueJob;

public interface IQueueJobHandler
{
    Task<Result<Guid>> QueueJobAsync(
        string userId,
        CancellationToken ct);
}