using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Domain.Job;
using WeatherPix.Infrastructure.Options.Table;
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
        var entity = new JobEntity
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
        var entity = await _tableClient.GetEntityAsync<JobEntity>(
            operationId.ToString(),
            JobRowKey,
            cancellationToken: ct);

        entity.Value.Status = jobStatus.ToString();

        await _tableClient.UpdateEntityAsync(
            entity.Value,
            entity.Value.ETag,
            TableUpdateMode.Replace,
            ct);
    }

    public async Task<Job?> GetJobAsync(
        Guid operationId,
        CancellationToken ct)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<JobEntity>(
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
        JobEntity entity)
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