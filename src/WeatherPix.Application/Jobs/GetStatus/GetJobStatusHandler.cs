using Microsoft.Extensions.Logging;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Common.Results;
using WeatherPix.Domain.Job;

namespace WeatherPix.Application.Jobs.GetStatus;

public class GetJobStatusHandler(
    IJobStatusRepository jobStatusRepository,
    ILogger<GetJobStatusHandler> logger)
    : IGetJobStatusHandler
{
    private readonly IJobStatusRepository _jobStatusRepository = jobStatusRepository;

    private readonly ILogger<GetJobStatusHandler> _logger = logger;

    public async Task<Result<Job?>> HandleAsync(
        Guid operationId,
        CancellationToken ct)
    {
        try
        {
            var job = await _jobStatusRepository.GetJobAsync(
                operationId,
                ct);

            return Result<Job?>.Success(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve job {OperationId}",
                operationId);

            return Result<Job?>.FailureWith(
                JobErrors.StatusRetrievalFailed);
        }
    }
}