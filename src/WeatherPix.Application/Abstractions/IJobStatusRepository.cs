using WeatherPix.Domain.Jobs;

namespace WeatherPix.Application.Abstractions;

public interface IJobStatusRepository
{
    Task CreateJobAsync(Job job, CancellationToken ct);
    Task MarkFailedAsync(Guid operationId, CancellationToken ct);
}