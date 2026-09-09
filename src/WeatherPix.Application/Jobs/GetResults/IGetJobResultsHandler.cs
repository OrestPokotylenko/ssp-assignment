using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.GetResults;

public interface IGetJobResultsHandler
{
    Task<Result<JobResultsData?>> HandleAsync(
        Guid operationId,
        CancellationToken ct);
}