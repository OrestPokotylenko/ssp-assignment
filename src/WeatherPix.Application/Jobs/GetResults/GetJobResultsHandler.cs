using Microsoft.Extensions.Logging;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.GetResults;

public class GetJobResultsHandler(
    IJobStatusRepository jobStatusRepository,
    IImageStorage imageStorage,
    ILogger<GetJobResultsHandler> logger)
    : IGetJobResultsHandler
{
    private readonly IJobStatusRepository _jobStatusRepository = jobStatusRepository;
    private readonly IImageStorage _imageStorage = imageStorage;
    private readonly ILogger<GetJobResultsHandler> _logger = logger;

    public async Task<Result<GetJobResultsResponse?>> HandleAsync(Guid operationId, CancellationToken ct)
    {
        try
        {
            var job = await _jobStatusRepository.GetJobAsync(operationId, ct);

            if (job is null)
                return Result<GetJobResultsResponse?>.Success(null);

            var images = await _imageStorage.GetImagesAsync(operationId, ct);

            return Result<GetJobResultsResponse?>.Success(
                new GetJobResultsResponse(
                    operationId,
                    job.Status,
                    images));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve results for operation {OperationId}",
                operationId);

            return Result<GetJobResultsResponse?>.FailureWith(
                JobErrors.ResultsRetrievalFailed);
        }
    }
}