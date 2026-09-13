using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.GetStatus;

public interface IGetJobStatusHandler
{
    Task<Result<JobStatusDetails?>> HandleAsync(
        string userId,
        Guid operationId,
        CancellationToken ct);
}