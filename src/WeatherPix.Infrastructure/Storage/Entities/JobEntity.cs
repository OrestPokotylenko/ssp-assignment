using Azure;
using Azure.Data.Tables;

namespace WeatherPix.Infrastructure.Storage.Entities;

public sealed class JobEntity : ITableEntity
{
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }

    public required string Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}