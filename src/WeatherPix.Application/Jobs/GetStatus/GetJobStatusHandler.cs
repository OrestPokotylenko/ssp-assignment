using Microsoft.Extensions.Logging;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.GetStatus;

public class GetJobStatusHandler(
    IJobStatusRepository jobStatusRepository,
    ILogger<GetJobStatusHandler> logger)
    : IGetJobStatusHandler
{
    private readonly IJobStatusRepository _jobStatusRepository = jobStatusRepository;

    private readonly ILogger<GetJobStatusHandler> _logger = logger;

    public async Task<Result<JobStatusDetails?>> HandleAsync(
        Guid operationId,
        CancellationToken ct)
    {
        try
        {
            var job = await _jobStatusRepository.GetJobAsync(
                operationId,
                ct);

            if (job is null)
            {
                return Result<JobStatusDetails?>.Success(null);
            }

            var progress = await _jobStatusRepository.GetProgressAsync(
                operationId,
                ct);

            return Result<JobStatusDetails?>.Success(
                new JobStatusDetails(
                    job,
                    progress));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve job {OperationId}",
                operationId);

            return Result<JobStatusDetails?>.FailureWith(
                JobErrors.StatusRetrievalFailed);
        }
    }
}