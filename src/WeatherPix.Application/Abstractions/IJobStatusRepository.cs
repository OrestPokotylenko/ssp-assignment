using WeatherPix.Domain.Job;

namespace WeatherPix.Application.Abstractions;

public interface IJobStatusRepository
{
    Task CreateJobAsync(
        Job job,
        CancellationToken ct);

    Task UpdateStatusAsync(
        Guid operationId,
        JobStatus status,
        CancellationToken ct);

    Task<Job?> GetJobAsync(
        Guid operationId,
        CancellationToken ct);

    Task CreateStationJobsAsync(
        Guid operationId,
        IReadOnlyCollection<int> stationIds,
        CancellationToken ct);

    Task UpdateStationStatusAsync(
        Guid operationId,
        int stationId,
        JobStatus status,
        CancellationToken ct);

    Task<JobProgress> GetProgressAsync(
        Guid operationId,
        CancellationToken ct);
}