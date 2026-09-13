using WeatherPix.Domain.Job;

namespace WeatherPix.Functions.Jobs.Contracts;

public sealed record JobStatusResponse(
    Guid OperationId,
    JobStatus Status,
    DateTimeOffset CreatedAt,
    JobProgress Progress);