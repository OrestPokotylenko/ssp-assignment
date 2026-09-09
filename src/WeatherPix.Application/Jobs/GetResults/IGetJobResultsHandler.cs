using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.GetResults;

public interface IGetJobResultsHandler
{
    Task<Result<GetJobResultsResponse?>> HandleAsync(
        Guid operationId,
        CancellationToken ct);
}