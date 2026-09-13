using Microsoft.Extensions.Logging;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Common.Results;
using WeatherPix.Application.Messaging;
using WeatherPix.Domain.Job;

namespace WeatherPix.Application.Jobs.QueueJob;

public class QueueJobHandler(
    IMessagePublisher messagePublisher,
    IJobStatusRepository jobStatusRepository,
    ILogger<QueueJobHandler> logger) : IQueueJobHandler
{
    private readonly IMessagePublisher _messagePublisher = messagePublisher;
    private readonly IJobStatusRepository _jobStatusRepository = jobStatusRepository;

    private readonly ILogger<QueueJobHandler> _logger = logger;

    public async Task<Result<Guid>> QueueJobAsync(
        string userId,
        CancellationToken ct)
    {
        Job job = new()
        {
            OwnerId = userId,
        };

        try
        {
            await _jobStatusRepository.CreateJobAsync(job, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
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
            var message = new StartJobMessage(job.OperationId);
            await _messagePublisher.PublishAsync(message, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish job {OperationId} to Service Bus",
                job.OperationId);

            try
            {
                await _jobStatusRepository.UpdateStatusAsync(
                    job.OperationId,
                    JobStatus.Failed,
                    ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
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