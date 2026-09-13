using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.GetResults;

public interface IGetJobResultsHandler
{
    Task<Result<JobResultsData?>> HandleAsync(
        string userId,
        Guid operationId,
        CancellationToken ct);
}