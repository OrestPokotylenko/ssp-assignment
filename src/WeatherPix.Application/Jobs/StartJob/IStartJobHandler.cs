using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.StartJob;

public interface IStartJobHandler
{
    Task<Result> HandleAsync(
        Guid operationId,
        CancellationToken ct);
}