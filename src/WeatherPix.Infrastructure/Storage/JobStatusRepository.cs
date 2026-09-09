using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Domain.Job;
using WeatherPix.Infrastructure.Options.Storage;
using WeatherPix.Infrastructure.Storage.Entities;

namespace WeatherPix.Infrastructure.Storage;

public class JobStatusRepository(
    TableServiceClient tableServiceClient,
    IOptions<StorageOptions> options)
    : IJobStatusRepository
{
    private readonly TableClient _tableClient =
        tableServiceClient.GetTableClient(options.Value.JobStatusTableName);

    private const string JobRowKey = "JOB";

    public async Task CreateJobAsync(
        Job job,
        CancellationToken ct)
    {
        var entity = new JobStatusEntity
        {
            PartitionKey = job.OperationId.ToString(),
            RowKey = JobRowKey,
            Status = job.Status.ToString(),
            CreatedAt = job.CreatedAt
        };

        await _tableClient.AddEntityAsync(entity, ct);
    }

    public async Task UpdateStatusAsync(
        Guid operationId,
        JobStatus jobStatus,
        CancellationToken ct)
    {
        var entity = new JobStatusUpdateEntity
        {
            PartitionKey = operationId.ToString(),
            RowKey = JobRowKey,
            Status = jobStatus.ToString(),
            ETag = ETag.All
        };

        await _tableClient.UpdateEntityAsync(
            entity,
            ETag.All,
            TableUpdateMode.Merge,
            ct);
    }

    public async Task CreateStationJobsAsync(
        Guid operationId,
        IReadOnlyCollection<int> stationIds,
        CancellationToken ct)
    {
        var partitionKey = operationId.ToString();

        var actions = stationIds
            .Select(stationId =>
                new TableTransactionAction(
                    TableTransactionActionType.Add,
                    new StationJobEntity
                    {
                        PartitionKey = partitionKey,
                        RowKey = $"STATION-{stationId}",
                        StationId = stationId,
                        Status = JobStatus.Queued.ToString()
                    }))
            .ToList();

        await _tableClient.SubmitTransactionAsync(actions, ct);
    }

    public async Task<JobProgress> GetProgressAsync(
        Guid operationId,
        CancellationToken ct)
    {
        var partitionKey = operationId.ToString();

        var entities = new List<StationJobEntity>();

        await foreach (var entity in _tableClient.QueryAsync<StationJobEntity>(
            x =>
                x.PartitionKey == partitionKey &&
                x.RowKey.CompareTo("STATION-") >= 0,
            cancellationToken: ct))
        {
            entities.Add(entity);
        }

        return new JobProgress(
            Total: entities.Count,
            Succeeded: entities.Count(x =>
                x.Status == JobStatus.Succeeded.ToString()),
            Failed: entities.Count(x =>
                x.Status == JobStatus.Failed.ToString()));
    }

    public async Task UpdateStationStatusAsync(
        Guid operationId,
        int stationId,
        JobStatus status,
        CancellationToken ct)
    {
        var entity = new StationJobEntity
        {
            PartitionKey = operationId.ToString(),
            RowKey = $"STATION-{stationId}",
            StationId = stationId,
            Status = status.ToString(),
            ETag = ETag.All
        };

        await _tableClient.UpdateEntityAsync(
            entity,
            ETag.All,
            TableUpdateMode.Merge,
            ct);
    }

    public async Task<Job?> GetJobAsync(
        Guid operationId,
        CancellationToken ct)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<JobStatusEntity>(
                operationId.ToString(),
                JobRowKey,
                cancellationToken: ct);

            return Map(response.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    private static Job Map(
        JobStatusEntity entity)
    {
        return new Job
        {
            OperationId = Guid.Parse(entity.PartitionKey),
            Status = Enum.Parse<JobStatus>(
                entity.Status,
                ignoreCase: true),
            CreatedAt = entity.CreatedAt
        };
    }
}