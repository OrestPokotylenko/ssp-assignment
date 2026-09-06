using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Common.Results;
using WeatherPix.Domain.Jobs;

namespace WeatherPix.Application.Jobs;

public class QueueJobHandler(
    IJobQueuePublisher queuePublisher, 
    IJobStatusRepository jobStatusRepository) : IQueueJobHandler
{
    private readonly IJobQueuePublisher _queuePublisher = queuePublisher;
    private readonly IJobStatusRepository _jobStatusRepository = jobStatusRepository;

    public async Task<Result<Guid>> QueueJobAsync(CancellationToken ct)
    {
        Job job = new();

        try
        {
            await _jobStatusRepository.CreateJobAsync(job, ct);
        }
        catch (Exception ex)
        {
            return Result<Guid>.FailureWith(new Error("Job.CreationFailed", ex.Message));
        }

        try
        {
            await _queuePublisher.PublishJobAsync(job, ct);
        }
        catch (Exception ex)
        {
            await _jobStatusRepository.MarkFailedAsync(
                job.OperationId,
                ct);

            return Result<Guid>.FailureWith(new Error("Job.CreationFailed", ex.Message));
        }

        return Result<Guid>.Success(job.OperationId);
    }
}