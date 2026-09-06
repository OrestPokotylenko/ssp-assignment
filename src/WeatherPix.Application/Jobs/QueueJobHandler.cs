using Microsoft.Extensions.Logging;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Common.Results;
using WeatherPix.Domain.Jobs;

namespace WeatherPix.Application.Jobs;

public class QueueJobHandler(
    IJobQueuePublisher queuePublisher,
    IJobStatusRepository jobStatusRepository,
    ILogger<QueueJobHandler> logger) : IQueueJobHandler
{
    private readonly IJobQueuePublisher _queuePublisher = queuePublisher;
    private readonly IJobStatusRepository _jobStatusRepository = jobStatusRepository;

    private readonly ILogger<QueueJobHandler> _logger = logger;

    public async Task<Result<Guid>> QueueJobAsync(CancellationToken ct)
    {
        Job job = new();

        try
        {
            await _jobStatusRepository.CreateJobAsync(job, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create job record for operation {OperationId}",
                job.OperationId);

            return Result<Guid>.FailureWith(
                JobErrors.CreationFailed);
        }

        try
        {
            await _queuePublisher.PublishJobAsync(job, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish job {OperationId} to Service Bus",
                job.OperationId);

            try
            {
                await _jobStatusRepository.MarkFailedAsync(
                    job.OperationId,
                    ct);
            }
            catch (Exception statusUpdateException)
            {
                _logger.LogError(
                    statusUpdateException,
                    "Failed to mark job {OperationId} as failed",
                    job.OperationId);
            }

            return Result<Guid>.FailureWith(
                JobErrors.QueueFailed);
        }

        return Result<Guid>.Success(job.OperationId);
    }
}