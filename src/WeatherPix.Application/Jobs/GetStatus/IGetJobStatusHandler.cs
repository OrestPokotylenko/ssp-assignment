using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.GetStatus;

public interface IGetJobStatusHandler
{
    Task<Result<JobStatusDetails?>> HandleAsync(
        Guid operationId,
        CancellationToken ct);
}