using WeatherPix.Application.Common.Results;
using WeatherPix.Domain.Job;

namespace WeatherPix.Application.Jobs.GetStatus;

public interface IGetJobStatusHandler
{
    Task<Result<Job?>> HandleAsync(
        Guid operationId,
        CancellationToken ct);
}