namespace WeatherPix.Domain.Job;

public record Job
{
    public Guid OperationId { get; init; } = Guid.NewGuid();
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public required string OwnerId { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}